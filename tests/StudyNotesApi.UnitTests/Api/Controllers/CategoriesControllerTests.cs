using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyNotesApi.Api.Controllers;
using StudyNotesApi.Api.DTOs.Categories;
using StudyNotesApi.Api.DTOs.Common;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Security;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Categories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Api.Controllers;

public class CategoriesControllerTests
{
    [Fact]
    public async Task GetAll_should_return_paged_categories_for_the_current_user()
    {
        var categoryService = new Mock<ICategoryService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new CategoriesController(categoryService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var category = new Category("Backend", userId, "#2563EB");
        var pagedResult = new PagedResult<Category>([category], 2, 5, 9);

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        categoryService.Setup(value => value.GetCategoriesAsync(
                userId,
                It.Is<CategoryFilter>(filter => filter.Search == "backend"),
                It.Is<PagedQuery>(query => query.PageNumber == 2 && query.PageSize == 5),
                It.Is<SortRequest>(sort => sort.SortBy == "name" && sort.Direction == SortDirection.Asc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await controller.GetAll(new GetCategoriesQueryDto
        {
            Search = "backend",
            PageNumber = 2,
            PageSize = 5,
            SortBy = "name",
            SortDirection = "asc"
        }, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResponseDto<CategoryResponseDto>>().Subject;

        response.Items.Should().ContainSingle();
        response.PageNumber.Should().Be(2);
        response.PageSize.Should().Be(5);
        response.TotalCount.Should().Be(9);
        response.TotalPages.Should().Be(2);
        response.HasPreviousPage.Should().BeTrue();
        response.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_should_throw_validation_exception_for_invalid_sort_direction()
    {
        var controller = new CategoriesController(Mock.Of<ICategoryService>(), Mock.Of<ICurrentUserService>());

        var action = async () => await controller.GetAll(new GetCategoriesQueryDto
        {
            SortDirection = "sideways"
        }, CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetAll_should_use_default_sort_direction_when_query_does_not_provide_one()
    {
        var categoryService = new Mock<ICategoryService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new CategoriesController(categoryService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        categoryService.Setup(value => value.GetCategoriesAsync(
                userId,
                It.IsAny<CategoryFilter>(),
                It.IsAny<PagedQuery>(),
                It.Is<SortRequest>(sort => sort.SortBy == "createdAt" && sort.Direction == SortDirection.Desc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Category>([], 1, 10, 0));

        var result = await controller.GetAll(new GetCategoriesQueryDto
        {
            SortBy = "createdAt"
        }, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_should_return_the_requested_category()
    {
        var categoryService = new Mock<ICategoryService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new CategoriesController(categoryService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var category = new Category("Backend", userId, "#2563EB");

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        categoryService.Setup(value => value.GetByIdAsync(userId, category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await controller.GetById(category.Id, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new CategoryResponseDto(
            category.Id,
            category.Name,
            category.Color,
            category.CreatedAt,
            category.UpdatedAt));
    }

    [Fact]
    public async Task Create_should_return_created_at_action_with_category_payload()
    {
        var categoryService = new Mock<ICategoryService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new CategoriesController(categoryService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var category = new Category("Backend", userId, "#2563EB");

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        categoryService.Setup(value => value.CreateAsync(userId, "Backend", "#2563EB", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await controller.Create(new CreateCategoryRequestDto("Backend", "#2563EB"), CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(CategoriesController.GetById));
        createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(category.Id);
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Value.Should().BeEquivalentTo(new CategoryResponseDto(
            category.Id,
            category.Name,
            category.Color,
            category.CreatedAt,
            category.UpdatedAt));
    }

    [Fact]
    public async Task Update_should_return_ok_with_updated_category()
    {
        var categoryService = new Mock<ICategoryService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new CategoriesController(categoryService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var category = new Category("Backend", userId, "#111111");

        category.Rename("Architecture");
        category.UpdateColor("#222222");

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        categoryService.Setup(value => value.UpdateAsync(userId, category.Id, "Architecture", "#222222", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await controller.Update(category.Id, new UpdateCategoryRequestDto("Architecture", "#222222"), CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new CategoryResponseDto(
            category.Id,
            category.Name,
            category.Color,
            category.CreatedAt,
            category.UpdatedAt));
    }

    [Fact]
    public async Task Delete_should_return_no_content_after_removing_category()
    {
        var categoryService = new Mock<ICategoryService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new CategoriesController(categoryService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);

        var result = await controller.Delete(categoryId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        categoryService.Verify(value => value.DeleteAsync(userId, categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
