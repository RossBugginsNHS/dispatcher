using Octokit;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.WorkflowRun;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


public partial class MyWebhookEventProcessor
{
    protected override async Task ProcessWorkflowRunWebhookAsync(
        WebhookHeaders headers,
        WorkflowRunEvent workflowRunEvent,
        WorkflowRunAction action)
    {
        if (action == WorkflowRunActionValue.Completed)
        {
            Logger.LogInformation("Got a webhook call of {Action} from {Owner}/{Repo}: {Workflow}",
                workflowRunEvent.Action,
                workflowRunEvent.Repository.Owner.Login,
                workflowRunEvent.Repository.Name,
                workflowRunEvent.Workflow.Name);
            var appClient = cs.GetAppClient();
            var install = await appClient.GitHubApps.GetInstallationForCurrent(workflowRunEvent.Installation.Id);
            var installClient = await cs.GetInstallationClient(appClient, install.Id);
            var file = await installClient.Repository.Content.GetAllContents(
                workflowRunEvent.Repository.Owner.Login,
                workflowRunEvent.Repository.Name,
                "dispatching.yml");
            var fileInfo = file.First();
            var content = fileInfo.Content;
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var p = deserializer.Deserialize<TriggersList>(content);

            foreach (var trigger in p.Triggers)
            {
                Logger.LogInformation("Looking at trigger {Source}", trigger.Source);
                if (workflowRunEvent.Workflow.Path.EndsWith(trigger.Source))
                {
                    foreach (var target in trigger.Targets)
                    {
                        await installClient.Actions.Workflows.CreateDispatch(
                              workflowRunEvent.Repository.Owner.Login,
                              target.Repository,
                              target.Workflow, new CreateWorkflowDispatch("main"));
                        var title = $"Triggered a dispatch of {target.Repository} {target.Workflow}";
                        var issue = await installClient.Issue.Create(
                            workflowRunEvent.Repository.Owner.Login,
                            workflowRunEvent.Repository.Name,
                            new NewIssue(title) { Body = content });
                        Logger.LogInformation("Created Issue {Issue}", issue.Id);
                    }
                }
            }
        }
    }
}
