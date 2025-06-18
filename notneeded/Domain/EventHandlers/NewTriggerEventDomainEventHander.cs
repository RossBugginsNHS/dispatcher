using MediatR;
using Org.BouncyCastle.Utilities.Zlib;

public class NewTriggerEventDomainEventHander(ILogger<NewTriggerEventDomainEventHander> Logger, ISourceTriggerRepository Sources, ITriggerer triggerer) : INotificationHandler<NewTriggerEvent>
{
  public Task Handle(NewTriggerEvent notification, CancellationToken cancellationToken)
  {
    Logger.LogInformation("Handing Event NewTriggerEvent");
    foreach (var x in notification.trigger.Targets)
    {
      triggerer.Trigger("Some Owner", x.Repository, x.Workflow);
    }
    Logger.LogInformation("Handled Event NewTriggerEvent");
    return Task.CompletedTask;
  }
}
