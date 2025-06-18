public record NewSourceEvent(Guid Id, CompletedWorkflowDetails CompletedWorkflowDetails, TriggeringUnitOfWork Repository) :IDomainEvent
{}
