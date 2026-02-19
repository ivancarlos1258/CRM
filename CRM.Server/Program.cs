using CRM.Application.Behaviors;
using CRM.Application.Services;
using CRM.Domain.Repositories;
using CRM.Infrastructure.Persistence;
using CRM.Infrastructure.Repositories;
using CRM.Infrastructure.Services;
using CRM.Server.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "CRM")
        .WriteTo.Console());

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CrmDatabase")));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IEventStore, EventStore>();

builder.Services.AddHttpClient<IZipCodeService, ViaCepService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CRM.Application.Common.ICommand<>).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(CRM.Application.Common.ICommand<>).Assembly);

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API v1");
    c.RoutePrefix = "swagger"; // Acesso em /swagger
    c.DocumentTitle = "CRM API - Documentação";
    c.DefaultModelsExpandDepth(-1); // Oculta schemas por padrão
});

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();

public partial class Program { }
