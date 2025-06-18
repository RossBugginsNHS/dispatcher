using MediatR;

public class NewSourceEventDomainEventHander(
  IMediator Mediator,
  ILogger<NewSourceEventDomainEventHander> Logger,
  ISourceTriggerRepository Sources) : INotificationHandler<NewSourceEvent>
{
  public async Task Handle(NewSourceEvent notification, CancellationToken cancellationToken)
  {
    Logger.LogInformation("Handling Event NewSourceEvent");
    var x = notification.CompletedWorkflowDetails;
    await Mediator.Send(new GetSourcesCommand(notification.Id, x));
    Logger.LogInformation("Handled Event NewSourceEvent");
  }
}
