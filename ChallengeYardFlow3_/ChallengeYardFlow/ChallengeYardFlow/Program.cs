using ChallengeYardFlow.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Entity Framework
builder.Services.AddDbContext<LocadoraContext>(opts =>
    opts.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração dos Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuração do Swagger - VERSÃO SIMPLES QUE FUNCIONA
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ChallengeYardFlow API",
        Description = "API para gerenciar motos de uma locadora"
    });
});

var app = builder.Build();

// Pipeline de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();