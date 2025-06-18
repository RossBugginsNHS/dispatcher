using MediatR;

public record GetSourcesCommand(Guid Id, CompletedWorkflowDetails Details):IRequest<Trigger>
{

}
