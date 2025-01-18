using Autofac.Core;
using GIC.BankingApplication.Application.Config;
using GIC.BankingApplication.Application.Infrastructure.MediatR;
using GIC.BankingApplication.Application.Services;
using GIC.BankingApplication.ConsoleApp.Presentation;
using GIC.BankingApplication.Infrastructure.Config;
using GIC.BankingApplication.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var host = Host.CreateDefaultBuilder(args)
     .ConfigureAppConfiguration((hostingContext, config) =>
     {
         config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
     })
     .ConfigureLogging((hostingContext, logging) =>
     {
         logging.AddConsole();
         logging.SetMinimumLevel(LogLevel.Information);
     })
    .ConfigureServices((hostContext, services) =>
    {
        //services.Configure<DatabaseSettings>(hostContext.Configuration.GetSection("Database"));

        services.AddDbContext<BankingApplicationDbContext>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(GIC.BankingApplication.Application.Config.IoC));
            //cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
        });

        services.RegisterApplication();
        services.RegisterInfrastructure(hostContext.Configuration);

        services.AddTransient<ITransactionService, TransactionService>();
        services.AddTransient<IInterestRuleService, InterestRuleService>();
        services.AddTransient<IStatementService, StatementService>();

        services.AddTransient<MenuHandler>();

    })
    .Build();

//host.Services.MigrateDbContext<BankingApplicationDbContext>((context, services) =>
//{
//    new PointCastDbContextSeed().SeedAsync(context).Wait();
//});

using var scope = host.Services.CreateScope();

var context = scope.ServiceProvider.GetService<BankingApplicationDbContext>();
context.Database.Migrate();

var menuHandler = scope.ServiceProvider.GetRequiredService<MenuHandler>();
menuHandler.ShowMainMenu();

