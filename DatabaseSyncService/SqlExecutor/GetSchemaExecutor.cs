using System.Data;
using System.Data.Common;
using System.Dynamic;
using Microsoft.Data.SqlClient;
using RepoDb;

namespace DatabaseSyncService.SqlExecutor;

public class GetSchemaExecutor : RawSqlExecutor
{
    public GetSchemaExecutor(DbConnection connection)
        : base(connection)
    {
    }

    public override IEnumerable<string> Execute(Dictionary<string, object> parameters = null)
    {
        Open();

        List<string> tableNames = new List<string>();
        var schema = _connection.GetSchema("Tables");
        foreach (DataRow row in schema.Rows)
        {
            if (row["TABLE_TYPE"].ToString() == "BASE TABLE")
            {
                string tableName = row[2].ToString();
                // 要確定有 Primary Key 才能進行同步
                var sql =
                    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = '{tableName}' AND CONSTRAINT_TYPE = 'PRIMARY KEY'";
                var result = _connection.ExecuteScalar<int>(sql);
                if (result >= 1) tableNames.Add(tableName);
            }
        }

        Close();
        return tableNames;
    }
}