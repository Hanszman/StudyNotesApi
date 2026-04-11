using StudyNotesApi.Api.Configurations;
using StudyNotesApi.Api.Extensions;

EnvironmentFileLoader.LoadFromSolutionRoot();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();
app.MapApiEndpoints();

app.Run();

public partial class Program;
