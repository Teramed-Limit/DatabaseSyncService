using System.Data;
using RepoDb;
using RepoDb.Interfaces;

namespace DatabaseSyncService.SqlExecutor.Tracer;

public class SQLTracer : ITrace
{
    private readonly ILogger<Worker> _logger;

    public SQLTracer(ILogger<Worker> logger)
    {
        _logger = logger;
    }


    public void BeforeExecution(CancellableTraceLog log)
    {
        // 將log的參數塞入statement中，並記錄日誌
        var result = log.Parameters.Aggregate(log.Statement,
            (current, parameter) => current.Replace(parameter.ParameterName, $"'{parameter.Value}'"));

        _logger.LogInformation($"BeforeExecution: {result}");
    }

    public void AfterExecution<TResult>(ResultTraceLog<TResult> log)
    {
        _logger.LogInformation("AfterExecution: {LogKey}, {LogResult}, ", log.Key, log.Result);
    }

    public async Task BeforeExecutionAsync(CancellableTraceLog log,
        CancellationToken cancellationToken = new CancellationToken())
    {
        // 將log的參數塞入statement中，並記錄日誌
        var result = log.Parameters.Aggregate(log.Statement,
            (current, parameter) => current.Replace(parameter.ParameterName, $"'{parameter.Value}'"));

        _logger.LogInformation($"BeforeExecutionAsync: {result}");
        await Task.CompletedTask;
    }

    public async Task AfterExecutionAsync<TResult>(ResultTraceLog<TResult> log,
        CancellationToken cancellationToken = new CancellationToken())
    {
        // 紀錄關於異步方式的執行之後資訊
        _logger.LogInformation("AfterExecutionAsync: {LogResult}", log.Result);
        await Task.CompletedTask;
    }
}