using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

public class TriggeringProcessAggreagateRoot : IRoot <TriggeringUnitOfWork>
{
  TriggeringUnitOfWork _unitOfWork;
  public TriggeringProcessAggreagateRoot(TriggeringUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  [JsonPropertyName("id")]
  [JsonInclude]
  public Guid Id { get; private set; } = Guid.NewGuid();

  [JsonPropertyName("completedWorkflowDetails")]
  [JsonInclude]
  public CompletedWorkflowDetails CompletedWorkflowDetails { get; private set; }

  [JsonPropertyName("trigger")]
  [JsonInclude]
  public Trigger Trigger { get; private set; }


  public void AddSource(CompletedWorkflowDetails details)
  {
    CompletedWorkflowDetails = details;
    _unitOfWork.RaiseEvent(new NewSourceEvent(Id, CompletedWorkflowDetails, _unitOfWork));
  }

  public void AddTrigger(Trigger triggers)
  {
    Trigger = triggers;
    _unitOfWork.RaiseEvent(new NewTriggerEvent(Id, Trigger, _unitOfWork));
  }
}
