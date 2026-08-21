using Gimnasio.Infrastructure;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, ".data-protection-keys")))
    .SetApplicationName("ProyectoAtlas");
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueDevelopment", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("VueDevelopment");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/system/status", () => Results.Ok(new
{
    application = "Proyecto Atlas",
    status = "ready",
    version = "0.3.0"
}));

app.Run();
