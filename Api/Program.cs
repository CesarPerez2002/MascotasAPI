using Api.Middleware;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Conexión a SQL Server
builder.Services.AddDbContext<MascotasDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Registrar repositorios
builder.Services.AddScoped<IEspecieRepository, EspecieRepository>();
builder.Services.AddScoped<IMascotaRepository, MascotaRepository>();

// Registrar servicios
builder.Services.AddScoped<IMascotaService, MascotaService>();

var app = builder.Build();

// Manejo centralizado de errores
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configuración de OpenAPI y Scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
