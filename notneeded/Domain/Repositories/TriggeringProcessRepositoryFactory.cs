public class TriggeringProcessRepositoryFactory
{
  IServiceProvider _provider;
  ObjectFactory<TriggeringProcessRepository> _factory;

  public TriggeringProcessRepositoryFactory(IServiceProvider provider)
  {
    _provider = provider;
    _factory = ActivatorUtilities.CreateFactory<TriggeringProcessRepository>(new[]
   {
      typeof(IServiceProvider),
      typeof(ILogger<TriggeringUnitOfWork>),
    typeof(IContext)
    });
  }

  public TriggeringProcessRepository Create(IContext context)
  {
    return  _factory(_provider, [_provider, context]);
  }


}
