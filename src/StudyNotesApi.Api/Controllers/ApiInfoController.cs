using Microsoft.AspNetCore.Mvc;
using StudyNotesApi.Api.DTOs.Common;

namespace StudyNotesApi.Api.Controllers;

[ApiController]
[Route("api")]
public class ApiInfoController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public ApiInfoController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet]
    [ProducesResponseType<ApiInfoResponse>(StatusCodes.Status200OK)]
    public ActionResult<ApiInfoResponse> Get()
    {
        var response = new ApiInfoResponse(
            Name: "StudyNotesApi",
            Version: "v1",
            Environment: _environment.EnvironmentName,
            DocumentationUrl: "/swagger",
            HealthUrl: "/api/health",
            TimestampUtc: DateTime.UtcNow);

        return Ok(response);
    }
}
