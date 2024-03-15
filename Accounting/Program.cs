using System.Net;
using Accounting;
using Accounting.Kafka;
using Accounting.Migrations;
using Accounting.Repositories;
using Core;
using Core.Kafka;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

var cfg = builder.Configuration.GetConnectionString("postgres");

builder.Services.AddCoreBase(builder.Configuration);

builder.Services.AddHostedService<DayOffCron>();
builder.Services.AddHostedService<RequestTimeV1Consumer>();
builder.Services.AddHostedService<AccountCreateConsumer>();
builder.Services.AddHostedService<AccountRoleChangeConsumer>();
builder.Services.AddHostedService<TaskCreateConsumer>();
builder.Services.AddHostedService<TaskCreateV2Consumer>();
builder.Services.AddHostedService<TaskAddedConsumer>();
builder.Services.AddHostedService<TaskAssignedConsumer>();
builder.Services.AddHostedService<TaskCompletedConsumer>();

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
    serverOptions.Listen(IPAddress.Any, 6002, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});

var app = builder.Build();
using var serviceScope = app.Services.CreateScope();
var services = serviceScope.ServiceProvider;
// Instantiate the runner
var runner = services.GetRequiredService<IMigrationRunner>();
// Run the migrations
runner.MigrateUp();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestTimerMiddleware>();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();