using Org.BouncyCastle.Utilities.Zlib;

public class TriggeringProcessService(
  TriggeringProcessRepositoryFactory sp,
   ISourceTriggerRepository Sources){
  public async Task<TriggeringProcessAggreagateRoot> AddSource(CompletedWorkflowDetails trigger)
  {
    var repository = sp.Create();
    var agg = repository.Create();
    agg.AddSource(trigger);
    await repository.Save();
    return agg;
  }

  public async Task<Trigger> AddTriggers(Guid id, string owner, string repository)
  {
      var trigger = Sources.Get(owner, repository);
     var repo = sp.Create();
    var item =repo.Get(id);
    item.AddTrigger(trigger);
    await repo.Save();
    return trigger;
  }
}
