using MediatR;

public class GetSourcesCommandHandler(
  TriggeringProcessService services,
  ILogger<GetSourcesCommandHandler> Logger) : IRequestHandler<GetSourcesCommand, Trigger>
{
  public async Task<Trigger> Handle(GetSourcesCommand request, CancellationToken cancellationToken)
  {
    Logger.LogInformation("Handling Command GetSourcesCommandHandler");
    var trigger = await services.AddTriggers(request.Id, request.Details.Owner, request.Details.Repository);
    Logger.LogInformation("Handled Command GetSourcesCommandHandler");
    return trigger;
  }
}
