using System.Collections.Concurrent;
using System.Text;
using Octokit;
using Octokit.Webhooks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

//await new Testing().TestItOut();

await RunWebApp.Run(args);


public class Testing
{
  public async Task TestItOut()
  {
    var contents = File.ReadAllText("dispatching.yml");
     IDeserializer Deserialiser = new DeserializerBuilder()
.WithNamingConvention(UnderscoredNamingConvention.Instance)
.Build();

    var lists = Deserialiser.Deserialize<TriggersList>(contents);
  }
}
