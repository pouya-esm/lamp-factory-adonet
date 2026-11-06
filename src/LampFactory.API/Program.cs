using LampFactory.API.Extensions;
using LampFactory.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Composition root: each layer registers its own services, and only this
// project knows that Infrastructure exists. AddInfrastructureServices is the
// single DI front door — validators of the Application assembly, repositories,
// current-user/token adapters, and the concrete service implementations.
builder.Services.AddControllers();
builder.Services.AddInfrastructureServices();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCustomCors();
builder.Services.AddCustomOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseApiMiddleware();

app.UseCors("AllowReactDev");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
