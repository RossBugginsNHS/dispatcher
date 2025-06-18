using MediatR;

public class TriggeringUnitOfWork : UnitOfWork
{
  public TriggeringUnitOfWork(IMediator mediator) : base(mediator)
  {

  }


  public TriggeringProcessRepository Repository { get; set; }
}
