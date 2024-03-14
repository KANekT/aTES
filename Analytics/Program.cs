using System.Net;
using Analytics.Kafka;
using Analytics.Migrations;
using Analytics.Repositories;
using Core;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

var cfg = builder.Configuration.GetConnectionString("postgres");

builder.Services.AddCoreBase(builder.Configuration);

builder.Services.AddHostedService<AccountCreateConsumer>();
builder.Services.AddHostedService<AccountRoleChangeConsumer>();
builder.Services.AddHostedService<TaskCreateConsumer>();
builder.Services.AddHostedService<TaskAssignedConsumer>();
builder.Services.AddHostedService<TaskCompletedConsumer>();
builder.Services.AddHostedService<TransactionCreateConsumer>();

// Add services to the container.
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

builder.Services.AddSingleton(new DapperContext(builder.Configuration));
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ITaskRepository, TaskRepository>();
builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<IConventionSet>(_ => new DefaultConventionSet(new CustomMetadataTable().SchemaName, null));
builder.Services
    .AddLogging(lb => lb.AddDebug().AddFluentMigratorConsole())
    .AddFluentMigratorCore()
    .ConfigureRunner(
        r => r
            .AddPostgres()
            .WithGlobalConnectionString(cfg)
            .ScanIn(typeof(CustomMetadataTable).Assembly).For.Migrations()
    );

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 6003, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();