using MediatR;

public interface IUnitOfWork
{
  IMediator Mediator { get; }
  void RaiseEvent(IDomainEvent domainEvent);

  Task<bool> OnSaving();

  Task OnSaved();

  Task Save();
}
