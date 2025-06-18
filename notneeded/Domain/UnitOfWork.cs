using MediatR;

public interface IContext
{
  IMediator Mediator { get; set; }

  Task Save();
}

public class JsonContext : IContext
{
  public IMediator Mediator { get; set; }

  public JsonContext(IMediator mediator)
  {
    Mediator = mediator;
  }

  public Task Save()
  {
    // Implement the logic to save the context, e.g., to a JSON file or database
    return Task.CompletedTask;
  }
}

public class DispatcherUnitOfWorkFactory
{
  IServiceProvider _provider;
  ObjectFactory<DispatcherUnitOfWork> _factory;

  public DispatcherUnitOfWorkFactory(IServiceProvider provider)
  {
    _provider = provider;
    _factory = ActivatorUtilities.CreateFactory<DispatcherUnitOfWork>(new[]
    {
      typeof(IMediator),
      typeof(ILogger<DispatcherUnitOfWork>),
      typeof(TriggeringProcessRepositoryFactory)
    });
  }

  public DispatcherUnitOfWork Create()
  {
    return _factory(_provider, [_provider.GetRequiredService<IMediator>(),
                                 _provider.GetRequiredService<ILogger<DispatcherUnitOfWork>>(),
                                 _provider.GetRequiredService<TriggeringProcessRepositoryFactory>()]);
  }

}
public class DispatcherUnitOfWork : UnitOfWork
{
  IContext _context;
  ILogger<DispatcherUnitOfWork> _logger;

  TriggeringProcessRepository _repository;
  public DispatcherUnitOfWork(
    IMediator mediator,
    ILogger<DispatcherUnitOfWork> logger,
    TriggeringProcessRepositoryFactory triggeringFactory) : base(mediator, logger)
  {
    _context = new JsonContext(mediator);
    _logger = logger;
    _repository = triggeringFactory.Create(_context);



  }

  public TriggeringProcessRepository Repository => _repository;



}


public abstract class UnitOfWork : IUnitOfWork
{
  ILogger<UnitOfWork> _logger;
  public IMediator Mediator { get; set; }

  public UnitOfWork(IMediator mediator, ILogger<UnitOfWork> logger)
  {
    _logger = logger;
    Mediator = mediator;
  }

  List<IDomainEvent> domainEvents = [];

  public void RaiseEvent(IDomainEvent domainEvent)
  {
    SetDirty();
    domainEvents.Add(domainEvent);
  }

  protected async Task PublishEvents()
  {
    while (domainEvents.Any())
    {
      var _domainEvents = domainEvents.ToArray();
      domainEvents.Clear();
      foreach (var domainEvent in _domainEvents)
        await Mediator.Publish(domainEvent);
    }
  }

  public async Task Save()
  {

    foreach (var item in tracked)
    {
      if (await item.item.OnSaving())
      {
        await Save(item.item);
        await item.item.OnSaved();
        _logger.LogInformation("Saved Item {ItemId}", item.item.Id);
      }
    }
  }


  public Task<bool> OnSaving()
  {
    return Task.FromResult(Dirty);
  }


  public async Task OnSaved()
  {
    await PublishEvents();
    SetClean();

  }

  public bool Dirty { get; private set; }

  private void SetDirty()
  {
    Dirty = true;
  }

  private void SetClean()
  {
    Dirty = false;
  }
}
