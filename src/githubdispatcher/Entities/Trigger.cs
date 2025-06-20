public class Trigger()
{
    public RepositoryWorkflow Source { get; set; }
    public RepositoryWorkflow[] Targets { get; set; } = new RepositoryWorkflow[0];
}
