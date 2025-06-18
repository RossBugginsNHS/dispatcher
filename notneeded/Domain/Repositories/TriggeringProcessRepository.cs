using Amazon.Lambda.AspNetCoreServer.Internal;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;

public interface IRepository
{

}

public record Tracker<TRoot, TUnitOfWork>(TRoot item, string stateWhenTracked)  where TRoot : IRoot<TUnitOfWork> where TUnitOfWork : IUnitOfWork;

public class TriggeringProcessRepository(
  IServiceProvider Provider,
  ILogger<TriggeringUnitOfWork> Logger,
  IContext context) : RepositoryBase<TriggeringProcessAggreagateRoot, TriggeringUnitOfWork>(Provider, Logger)
{


}
