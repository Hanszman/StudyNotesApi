using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyNotesApi.Api.Controllers;
using StudyNotesApi.Api.DTOs.Common;
using StudyNotesApi.Api.DTOs.Tags;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Security;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Tags;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Api.Controllers;

public class TagsControllerTests
{
    [Fact]
    public async Task GetAll_should_return_paged_tags_for_the_current_user()
    {
        var tagService = new Mock<ITagService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new TagsController(tagService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var tag = new Tag("dotnet", userId);
        var pagedResult = new PagedResult<Tag>([tag], 2, 5, 9);

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        tagService.Setup(value => value.GetTagsAsync(
                userId,
                It.Is<TagFilter>(filter => filter.Search == "dotnet"),
                It.Is<PagedQuery>(query => query.PageNumber == 2 && query.PageSize == 5),
                It.Is<SortRequest>(sort => sort.SortBy == "name" && sort.Direction == SortDirection.Asc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await controller.GetAll(new GetTagsQueryDto
        {
            Search = "dotnet",
            PageNumber = 2,
            PageSize = 5,
            SortBy = "name",
            SortDirection = "asc"
        }, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResponseDto<TagResponseDto>>().Subject;

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
        var controller = new TagsController(Mock.Of<ITagService>(), Mock.Of<ICurrentUserService>());

        var action = async () => await controller.GetAll(new GetTagsQueryDto
        {
            SortDirection = "sideways"
        }, CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetAll_should_use_default_sort_direction_when_query_does_not_provide_one()
    {
        var tagService = new Mock<ITagService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new TagsController(tagService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        tagService.Setup(value => value.GetTagsAsync(
                userId,
                It.IsAny<TagFilter>(),
                It.IsAny<PagedQuery>(),
                It.Is<SortRequest>(sort => sort.SortBy == "createdAt" && sort.Direction == SortDirection.Desc),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Tag>([], 1, 10, 0));

        var result = await controller.GetAll(new GetTagsQueryDto
        {
            SortBy = "createdAt"
        }, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_should_return_the_requested_tag()
    {
        var tagService = new Mock<ITagService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new TagsController(tagService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var tag = new Tag("dotnet", userId);

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        tagService.Setup(value => value.GetByIdAsync(userId, tag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await controller.GetById(tag.Id, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new TagResponseDto(
            tag.Id,
            tag.Name,
            tag.CreatedAt,
            tag.UpdatedAt));
    }

    [Fact]
    public async Task Create_should_return_created_at_action_with_tag_payload()
    {
        var tagService = new Mock<ITagService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new TagsController(tagService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var tag = new Tag("dotnet", userId);

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        tagService.Setup(value => value.CreateAsync(userId, "dotnet", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await controller.Create(new CreateTagRequestDto("dotnet"), CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(TagsController.GetById));
        createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(tag.Id);
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Value.Should().BeEquivalentTo(new TagResponseDto(
            tag.Id,
            tag.Name,
            tag.CreatedAt,
            tag.UpdatedAt));
    }

    [Fact]
    public async Task Update_should_return_ok_with_updated_tag()
    {
        var tagService = new Mock<ITagService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new TagsController(tagService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var tag = new Tag("dotnet", userId);

        tag.Rename("csharp");

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);
        tagService.Setup(value => value.UpdateAsync(userId, tag.Id, "csharp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await controller.Update(tag.Id, new UpdateTagRequestDto("csharp"), CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new TagResponseDto(
            tag.Id,
            tag.Name,
            tag.CreatedAt,
            tag.UpdatedAt));
    }

    [Fact]
    public async Task Delete_should_return_no_content_after_removing_tag()
    {
        var tagService = new Mock<ITagService>();
        var currentUserService = new Mock<ICurrentUserService>();
        var controller = new TagsController(tagService.Object, currentUserService.Object);
        var userId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        currentUserService.Setup(value => value.GetRequiredUserId()).Returns(userId);

        var result = await controller.Delete(tagId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        tagService.Verify(value => value.DeleteAsync(userId, tagId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
