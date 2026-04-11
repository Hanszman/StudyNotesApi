using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Moq;
using StudyNotesApi.Api.Controllers;
using StudyNotesApi.Api.DTOs.Common;

namespace StudyNotesApi.UnitTests.Api.Controllers;

public class ApiInfoControllerTests
{
    [Fact]
    public void Get_should_return_api_metadata()
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.EnvironmentName).Returns(Environments.Development);
        var controller = new ApiInfoController(environment.Object);

        var result = controller.Get();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiInfoResponse>().Subject;

        response.Name.Should().Be("StudyNotesApi");
        response.Version.Should().Be("v1");
        response.Environment.Should().Be(Environments.Development);
        response.DocumentationUrl.Should().Be("/swagger");
        response.HealthUrl.Should().Be("/api/health");
    }
}
