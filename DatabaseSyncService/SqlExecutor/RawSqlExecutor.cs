using System.Data;
using System.Data.Common;
using System.Dynamic;

namespace DatabaseSyncService.SqlExecutor;

public abstract class RawSqlExecutor
{
    protected readonly DbConnection _connection;

    protected RawSqlExecutor(DbConnection connection)
    {
        _connection = connection;
    }

    public abstract IEnumerable<dynamic> Execute(Dictionary<string, object> parameters = null);

    public void Open()
    {
        if (_connection.State == ConnectionState.Open) return;
        _connection.Open();
    }

    public void Close()
    {
        if (_connection.State != ConnectionState.Open) return;
        _connection.Close();
        _connection.Dispose();
    }
}