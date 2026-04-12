using StudyNotesApi.Api.Middlewares;

namespace StudyNotesApi.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => Results.Redirect("/swagger"))
            .AllowAnonymous()
            .ExcludeFromDescription();

        app.MapControllers();
        return app;
    }
}
