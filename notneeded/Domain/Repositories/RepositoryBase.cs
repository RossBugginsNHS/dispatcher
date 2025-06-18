using System.Text.Json;

public abstract class RepositoryBase<TRoot, TUnitOfWork>(
  IServiceProvider Provider,
  ILogger<TUnitOfWork> Logger) : IRepository where TRoot : IRoot<TUnitOfWork>
  where TUnitOfWork : IUnitOfWork
{
   private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
  {
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
  };

  protected Dictionary<Guid, string> saved = [];
  protected List<Tracker<TRoot, TUnitOfWork>> tracked = [];

  public TRoot Get(Guid id)
  {
    if (tracked.Any(x => x.item.Id == id))
      return tracked.Single(s => s.item.Id == id).item;
    else
    {
      var itemStr = saved.Single(s => s.Key == id).Value;
      var item = JsonSerializer.Deserialize<TRoot>(itemStr, JsonSerializerOptions);
      Logger.LogInformation("Loaded Item {ItemId}", item.Id);
      AddTracking(itemStr, item);
      return tracked.Single(s => s.item.Id == id).item;
    }
  }

  protected void AddTracking(string stateAtLoad, TRoot item)
  {
    tracked.Add(new Tracker<TRoot, TUnitOfWork>(item, stateAtLoad));
    Logger.LogInformation("Tracking Item {ItemId}", item.Id);
  }

  protected void AddTracking(TRoot item)
  {
    var str = JsonSerializer.Serialize(item);
    AddTracking(str, item);
  }

  public TRoot Create()
  {
    var item = ActivatorUtilities.CreateInstance<TRoot>(Provider);
    Logger.LogInformation("Created Item {ItemId}", item.Id);
    AddTracking(item);
    return item;
  }



    private Task<bool> Save(TRoot item)
  {
    var itemStr = JsonSerializer.Serialize(item, JsonSerializerOptions);
    if (saved.TryAdd(item.Id, itemStr))
    {
      Logger.LogInformation("Saved New Item {ItemId}", item.Id);
      return Task.FromResult(true);
    }
    else
    {

      if (saved[item.Id] == tracked.Single(x => x.item.Id == item.Id).stateWhenTracked)
      {
        Logger.LogInformation("Overwritten existing New Item {ItemId}", item.Id);
        saved[item.Id] = itemStr;
        return Task.FromResult(true);
      }
      else
      {
        Logger.LogError("Failed to overwrite {ItemId} as it has changed since tracking started", item.Id);
        return Task.FromResult(false);
      }

    }
  }
}
