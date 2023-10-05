using System.Data.Common;

namespace DatabaseSyncService.Helper;

public class ConnectionFactory
{
    private readonly Dictionary<string, Func<DbConnection>> _factories = new();

    public void Add(string name, Func<DbConnection> factory)
    {
        _factories[name] = factory;
    }

    public DbConnection GetConnection(string name)
    {
        if (_factories.TryGetValue(name, out var factory))
        {
            return factory.Invoke();
        }

        throw new ArgumentException($"No connection registered with the name {name}");
    }
}