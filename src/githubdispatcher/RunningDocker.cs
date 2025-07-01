using System.Diagnostics;
using System.Net;
using System.Text.Json;
using static GithubWorkflowEventProcessor;

public class RunningDocker(ILogger<RunningDocker> logger)
{
  static string terraformrootdir  = "terraform";
  static string terraformstatedir = "tfstates";
  static string terraformcommand = "terraform";
  static string planfile = "main.tfplan";
  static string applyCommand = $"apply {planfile}";
  static string initCommand = $"init";
  static string tfFileName = "hello.tf";
  static string reposFile = "repos.json";
  static string pemFileName = "robu6-dispatcher.2024-11-14.private-key.pem";
  public async Task InitTF(string owner, RepoVending repos)
  {
    var appDirectory = Directory.GetCurrentDirectory();

    var helloFile = Path.Combine(appDirectory, terraformrootdir, tfFileName);
    var pemfile = Path.Combine(appDirectory, terraformrootdir, pemFileName);
    var ownersTFDirectory = Path.Combine(appDirectory, terraformstatedir, owner);

    logger.LogInformation("Initializing Terraform for owner: {Owner}", owner);
    logger.LogInformation("Owners TF Directory: {OwnersTFDirectory}", ownersTFDirectory);

    if (!Directory.Exists(ownersTFDirectory))
    {
      Directory.CreateDirectory(ownersTFDirectory);
      logger.LogInformation("Created directory: {OwnersTFDirectory}", ownersTFDirectory);
    }
    else
    {
      logger.LogInformation("Directory already exists: {OwnersTFDirectory}", ownersTFDirectory);
    }

    var options = new JsonSerializerOptions
    {
      WriteIndented = false,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    var repoStr = JsonSerializer.Serialize(repos, options);
    File.WriteAllText(Path.Combine(ownersTFDirectory, reposFile), repoStr);

    File.Copy(helloFile, Path.Combine(ownersTFDirectory, tfFileName), true);
    File.Copy(pemfile, Path.Combine(ownersTFDirectory, pemFileName), true);

    var processInfo = new ProcessStartInfo(terraformcommand, initCommand);
    processInfo.WorkingDirectory = ownersTFDirectory;
    processInfo.CreateNoWindow = true;
    processInfo.UseShellExecute = false;
    processInfo.RedirectStandardOutput = true;
    processInfo.RedirectStandardError = true;

    int exitCode;
    using (var process = new Process())
    {
      process.StartInfo = processInfo;
      process.OutputDataReceived += new DataReceivedEventHandler(logOrWhatever);
      process.ErrorDataReceived += new DataReceivedEventHandler(logOrWhatever);

      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      process.WaitForExit(1200000);
      if (!process.HasExited)
      {
        process.Kill();
      }

      exitCode = process.ExitCode;
      process.Close();
    }
  }


  public async Task PlanTf(string owner, string app_id, string app_installation_id, string pem_file)
  {
    var appDirectory = Directory.GetCurrentDirectory();
    //var helloFile = Path.Combine(appDirectory, terraformrootdir, tfFileName);
    //var pemfile = Path.Combine(appDirectory, terraformrootdir, pemFileName);
    var ownersTFDirectory = Path.Combine(appDirectory, terraformstatedir, owner);

    var options = new JsonSerializerOptions
    {
      WriteIndented = false,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    var reposJson = File.ReadAllText(Path.Combine(ownersTFDirectory, reposFile));
    reposJson = reposJson.Replace("\"", "\\\"");


    var cmd = $"plan -out={planfile} -input=false -var owner={owner} -var app_id={app_id} -var app_installation_id={app_installation_id} -var pem_file={pem_file} -var repos=\"{reposJson}\"";
    logger.LogInformation("Running command: docker {Command}", cmd);
    var processInfo = new ProcessStartInfo(
      terraformcommand, cmd
   );

    processInfo.WorkingDirectory = ownersTFDirectory;
    processInfo.CreateNoWindow = false;
    processInfo.UseShellExecute = false;
    processInfo.RedirectStandardOutput = true;
    processInfo.RedirectStandardError = true;

    processInfo.EnvironmentVariables["TF_LOG"] = "DEBUG";

    int exitCode;
    using (var process = new Process())
    {
      process.StartInfo = processInfo;
      process.OutputDataReceived += new DataReceivedEventHandler(logOrWhatever);
      process.ErrorDataReceived += new DataReceivedEventHandler(logOrWhatever);

      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      process.WaitForExit(1200000);
      if (!process.HasExited)
      {
        process.Kill();
      }

      exitCode = process.ExitCode;
      process.Close();
    }
  }

  public async Task ApplyTf(string owner)
  {
    var appDirectory = Directory.GetCurrentDirectory();
    //var helloFile = Path.Combine(appDirectory, terraformrootdir, tfFileName);
    //var pemfile = Path.Combine(appDirectory, terraformrootdir, pemFileName);
    var ownersTFDirectory = Path.Combine(appDirectory, terraformstatedir, owner);

    var cmd = applyCommand;
    var processInfo = new ProcessStartInfo(terraformcommand, cmd);
    processInfo.WorkingDirectory = ownersTFDirectory;
    processInfo.CreateNoWindow = true;
    processInfo.UseShellExecute = false;
    processInfo.RedirectStandardOutput = true;
    processInfo.RedirectStandardError = true;

    int exitCode;
    using (var process = new Process())
    {
      process.StartInfo = processInfo;
      process.OutputDataReceived += new DataReceivedEventHandler(logOrWhatever);
      process.ErrorDataReceived += new DataReceivedEventHandler(logOrWhatever);

      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      process.WaitForExit(1200000);
      if (!process.HasExited)
      {
        process.Kill();
      }

      exitCode = process.ExitCode;
      process.Close();
    }
  }


  private void logOrWhatever(object sender, System.Diagnostics.DataReceivedEventArgs e)
  {
    logger.LogInformation(e.Data);
  }

}
