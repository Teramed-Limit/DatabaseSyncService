namespace DatabaseSyncService.Model;

public class ChangeData
{
    public string Operation { get; }
    public Dictionary<string, object> Data { get; }

    public ChangeData(string operation, Dictionary<string, object> data)
    {
        Operation = operation;
        Data = data;
    }
}