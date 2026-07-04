using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TechmartAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Встроенный OpenApi для .NET 10

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Настройка OpenApi
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var webPath = Path.Combine(Directory.GetCurrentDirectory(), "../Web");
if (Directory.Exists(webPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(webPath),
        RequestPath = ""
    });
}
else
{
    app.UseStaticFiles();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();