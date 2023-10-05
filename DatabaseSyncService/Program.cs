using System.Data.SqlClient;
using DatabaseSyncService;
using DatabaseSyncService.Helper;
using DatabaseSyncService.Logic;
using DatabaseSyncService.Model;
using DatabaseSyncService.SqlExecutor;
using DatabaseSyncService.SqlExecutor.Tracer;
using Microsoft.Extensions.Options;
using RepoDb;
using RepoDb.DbHelpers;
using RepoDb.DbSettings;
using RepoDb.StatementBuilders;
using Serilog;

// 我們在測試時的執行檔案是在: \bin\Debug\net6.0\Service.exe
// 但是測試時程式啟動後預設的路徑卻是: Service\Service
// 這樣會讓我們輸出的 log 位置不正確，需要在程式一開始執行時就將預設目錄設定為程式執行檔的那個目錄
Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

// 註冊 SqlServer 連接
GlobalConfiguration.Setup().UseSqlServer();
var dbSetting = new SqlServerDbSetting();
DbSettingMapper.Add<SqlConnection>(dbSetting, true);
DbHelperMapper.Add<SqlConnection>(new SqlServerDbHelper(), true);
StatementBuilderMapper.Add<SqlConnection>(new SqlServerStatementBuilder(dbSetting), true);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.log",
        rollingInterval: RollingInterval.Day, // 每小時一個檔案
        outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u5}] {Message:lj}{NewLine}{Exception}",
        flushToDiskInterval: TimeSpan.FromSeconds(5)
    )
    .CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .UseWindowsService(options => { options.ServiceName = "Database Sync Service"; })
    .ConfigureServices((hostContext, services) =>
    {
        // Configure your DatabaseSettings from appsettings.json or another configuration source
        services.Configure<DatabaseSettings>(hostContext.Configuration.GetSection("DatabaseSettings"));
        services.AddSingleton<ConnectionFactory>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            var factory = new ConnectionFactory();
            factory.Add("SourceConnection",
                () => new Microsoft.Data.SqlClient.SqlConnection(
                    DatabaseHelper.ToConnectionString(settings.SourceDBSettings)));
            factory.Add("TargetConnection",
                () => new Microsoft.Data.SqlClient.SqlConnection(
                    DatabaseHelper.ToConnectionString(settings.TargetDBSettings)));
            return factory;
        });

        services.AddSingleton<ChangeTrackingManager>();
        services.AddSingleton<TableSyncFilter>();
        services.AddTransient<SyncVersionManager>();
        services.AddTransient<DatabaseSynchronizer>();
        services.AddTransient<SQLTracer>();

        services.AddTransient<ExecutorFactory>();
        services.AddTransient<RepoDbWriter>();
        // Worker
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();