using DatabaseSyncService.Model;
using DatabaseSyncService.SqlExecutor;
using Microsoft.Extensions.Options;

namespace DatabaseSyncService.Logic;

public class TableSyncFilter
{
    private readonly ExecutorFactory _executorFactory;
    private readonly DatabaseSettings _databaseSettings;
    private List<string> _filteredTables = new();
    private bool _alreadyFiltered = false;

    public TableSyncFilter(ExecutorFactory executorFactory, IOptions<DatabaseSettings> databaseSetting)
    {
        _executorFactory = executorFactory;
        _databaseSettings = databaseSetting.Value;
    }


    // 根據TableSyncMode篩選表名
    public List<string> FilterTablesBasedOnSyncMode()
    {
        if (_alreadyFiltered) return _filteredTables;

        var executor = _executorFactory.CreateGetSchemaExecutor("SourceConnection");
        var allTables = executor.Execute();

        var tablesToIncludeOrExclude = _databaseSettings?.TableNames ?? new List<string>();
        if (!tablesToIncludeOrExclude.Any())
        {
            _alreadyFiltered = true;
            _filteredTables = (List<string>)allTables;
            return _filteredTables;
        }

        // 根據不同的模式進行篩選
        if (_databaseSettings == null)
        {
            _alreadyFiltered = true;
            return new List<string>();
        }

        switch (_databaseSettings.TableSyncMode)
        {
            case TableSyncMode.All:
                // 返回所有表名
                break;
            case TableSyncMode.Include:
                // 只返回包含在tablesToIncludeOrExclude列表中的表名
                _filteredTables = allTables.Where(table => tablesToIncludeOrExclude.Contains(table)).ToList();
                break;
            case TableSyncMode.Exclude:
                // 返回不包含在tablesToIncludeOrExclude列表中的表名
                _filteredTables = allTables.Where(table => !tablesToIncludeOrExclude.Contains(table)).ToList();
                break;
            default:
                // 返回空列表
                _filteredTables = new List<string>();
                break;
        }

        _alreadyFiltered = true;
        return _filteredTables;
    }
}