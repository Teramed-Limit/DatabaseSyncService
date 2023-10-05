using System.Data;
using System.Data.Common;
using System.Dynamic;
using RepoDb;

namespace DatabaseSyncService.SqlExecutor;

public class QueryCTExecutor : RawSqlExecutor
{
    public QueryCTExecutor(DbConnection connection)
        : base(connection)
    {
    }

    public override IEnumerable<dynamic> Execute(Dictionary<string, object> parameters = null)
    {
        Open();

        // 檢查parameters
        if (parameters == null || !parameters.ContainsKey("tableName")
                               || !parameters.ContainsKey("lastSyncVersion")
                               || !parameters.ContainsKey("operation"))
            throw new ArgumentException("parameters must contain tableName");

        var tableName = parameters["tableName"].ToString();
        var lastSyncVersion = parameters["lastSyncVersion"].ToString();
        var operation = parameters["operation"].ToString();

        var helper = _connection.GetDbHelper();
        var dbFields = helper.GetFields(_connection, tableName);
        var primaryFields = dbFields.Where(x => x.IsPrimary);
        var conditionStr = string.Join(" and ", primaryFields.Select(col => $"CT.{col.Name} = T.{col.Name}"));

        var query = "";
        if (operation == "Merge")
        {
            query = $@"SELECT CT.SYS_CHANGE_OPERATION, T.*
                      FROM CHANGETABLE(CHANGES {tableName}, {lastSyncVersion}) AS CT
                      LEFT JOIN {tableName} T ON {conditionStr} 
                      Where SYS_CHANGE_OPERATION != 'D'";
        }
        else
        {
            query = $@"SELECT *
                      FROM CHANGETABLE(CHANGES {tableName}, {lastSyncVersion}) AS CT
                      Where SYS_CHANGE_OPERATION = 'D'";
        }
        
        var result = _connection.ExecuteQuery<dynamic>(query, parameters);

        Close();
        return result;
    }
}