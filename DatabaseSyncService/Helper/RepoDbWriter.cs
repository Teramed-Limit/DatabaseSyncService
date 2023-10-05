using System.Data;
using System.Data.Common;
using System.Dynamic;
using DatabaseSyncService.Helper;
using DatabaseSyncService.Model;
using DatabaseSyncService.SqlExecutor.Tracer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using RepoDb;
using RepoDb.Interfaces;

public class RepoDbWriter
{
    private readonly ConnectionFactory _connectionFactory;
    private DbConnection _connection;
    private IDbHelper _dbHelper;
    private SQLTracer _sqlTracer;


    public RepoDbWriter(ConnectionFactory connectionFactory, SQLTracer sqlTracer)
    {
        _connectionFactory = connectionFactory;
        _sqlTracer = sqlTracer;
    }

    public void Connect(string connectionName)
    {
        _connection = _connectionFactory.GetConnection(connectionName);
        _dbHelper = _connection.GetDbHelper();
    }

    public void Close()
    {
        if (_connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }

    private dynamic CreateDynamicObject(string tableName, Dictionary<string, object> values)
    {
        var dbFields = _dbHelper.GetFields(_connection, tableName);
        dynamic expando = new ExpandoObject();
        var expandoDict = expando as IDictionary<string, object>;

        foreach (var field in dbFields)
        {
            if (values.ContainsKey(field.Name))
            {
                expandoDict[field.Name] = values[field.Name];
            }
            else
            {
                expandoDict[field.Name] = null;
            }
        }

        return expando;
    }

    public object QueryFirst(string tableName, Dictionary<string, object> values)
    {
        var record = CreateDynamicObject(tableName, values);
        return _connection.Query(tableName, (object)record, trace: _sqlTracer).FirstOrDefault();
    }

    public object Query(string tableName, Dictionary<string, object> values)
    {
        var record = CreateDynamicObject(tableName, values);
        return _connection.Query(tableName, (object)record, trace: _sqlTracer);
    }

    public object Upsert(string tableName, Dictionary<string, object> values)
    {
        var record = CreateDynamicObject(tableName, values);
        return _connection.Merge(tableName, (object)record, trace: _sqlTracer);
    }

    public int Delete(string tableName, Dictionary<string, object> values)
    {
        // 檢查是否有主鍵
        var primaryField = _dbHelper.GetFields(_connection, tableName).Where(x => x.IsPrimary);
        if (!primaryField.Any()) return -1;

        var queryFields = primaryField.Select(p => new QueryField(p.Name, values[p.Name]));
        return _connection.Delete(tableName, where: queryFields, trace: _sqlTracer);
    }
}