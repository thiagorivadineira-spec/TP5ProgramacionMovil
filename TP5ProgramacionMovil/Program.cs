using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Data;

var builder = WebApplication.CreateBuilder(args);

// Obtener la cadena de conexión de appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrar el DataContext para usar SQL Server
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));

// Controladores
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS permisivo
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPermisivo", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
var app = builder.Build();

// Swagger en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPermisivo");

// Habilitar archivos estáticos de wwwroot
app.UseStaticFiles();

// Crear la carpeta wwwroot/uploads si no existe
var uploadsPath = Path.Combine(
    app.Environment.WebRootPath
        ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
    "uploads"
);

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseAuthorization();

app.MapControllers();

app.Run();