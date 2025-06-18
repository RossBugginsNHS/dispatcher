public class SourceTriggerRepository : ISourceTriggerRepository
{
  public Trigger Get(string owner, string repository)
  {
    return new Trigger(){Source=repository};
  }
}
