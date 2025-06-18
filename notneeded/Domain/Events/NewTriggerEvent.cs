public record NewTriggerEvent(Guid Id, Trigger trigger, TriggeringUnitOfWork Repository) :IDomainEvent
{}
