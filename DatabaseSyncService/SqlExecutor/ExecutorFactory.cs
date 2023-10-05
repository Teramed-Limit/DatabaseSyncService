using DatabaseSyncService.Helper;
using DatabaseSyncService.Model;
using Microsoft.Extensions.Options;

namespace DatabaseSyncService.SqlExecutor;

public class ExecutorFactory
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly DatabaseSettings _databaseSettings;

    public ExecutorFactory(ConnectionFactory connectionFactory, IOptions<DatabaseSettings> settings)
    {
        _connectionFactory = connectionFactory;
        _databaseSettings = settings.Value;
    }

    public EnableDatabaseCTExecutor CreateEnableDatabaseCTExecutor(string connectionName)
    {
        var connection = _connectionFactory.GetConnection(connectionName);
        return new EnableDatabaseCTExecutor(connection, _databaseSettings);
    }

    public EnableTableCTExecutor CreateEnableTableCTExecutor(string connectionName)
    {
        var connection = _connectionFactory.GetConnection(connectionName);
        return new EnableTableCTExecutor(connection);
    }

    public QueryCTExecutor CreateQueryCTExecutor(string connectionName)
    {
        var connection = _connectionFactory.GetConnection(connectionName);
        return new QueryCTExecutor(connection);
    }

    public GetSchemaExecutor CreateGetSchemaExecutor(string connectionName)
    {
        var connection = _connectionFactory.GetConnection(connectionName);
        return new GetSchemaExecutor(connection);
    }
}