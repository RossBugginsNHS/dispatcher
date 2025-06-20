# ASPDOTNET Github dispatcher app

This app can be used to trigger workflows in a repository (the target) after they have fished in another (the source).

* The app needs installing in any repositories that are going to be sources of triggers, and also any that are going to be targets.

## Live app deployment / availability

* This app is  running as a live PoC Github app hosted on azure. NOT FOR PRODUCTION.
* It can need self hosting and deploying in github for evaluation.

## What code should I look at to see how this works?

Look at [WebhookEventProcessor.cs](src/githubdispatcher/Processors/WebhookEventProcessor.cs) to see the basics of what this does

## Github dispatcher specifics

* Register app in GitHub
* install into both source and destination repos
* in source, create a `dispatching.yml` in the root.
* when the source workflow completes, it will trigger the target workflow in the target repository (in the same org).

## App design

* Dotnet 8 project

### Example dispatching file

#### Source

This would be in the source repo. When the app detects a workflow finished, it looks for this and then will do any
required dispatching.

```yml
outbound:
  - source:
      workflow: ci.yml
    targets:
      - repository: my-target-repo
        workflow: cd.yml


```

#### Target

This would be in the target repo. When the app detects a workflow finished, it looks for this and then will do any
required dispatching.

```yml

inbound:
  - source:
      repository: my-source-repo
      workflow: ci.yml
    targets:
      -  workflow: cd.yml

```

#### Chained

They can be chained, eg the initial target could have:

```yml

inbound:
  - source:
      repository: my-source-repo
      workflow: ci.yml
    targets:
      -  workflow: cd.yml

outbound:
  - source:
      workflow: cd.yml
    targets:
      - repository: my-target-repo-2
        workflow: cd-2.yml


```

## Todos

* Currently this soruce repo controlls what external workflows it will trigger
* Could easily add it that it could be destination based config
* Add config for both source and target repos to control if they want their workflows triggering by specific repos
* Check to see if .NET AOT and trimming work with this to improve cold starts

### Local development

* Need to register a github app
* Generate a private key
* Configure github app with permissions of Dispatch Read/Write and Issue Read/Write
* Use ngrok or similar
* dotnet F5 debug in vscode
* rename .env.txt to .env and set values accordingly to your github app instance.

### Dev details

* dotnet 8 project
* deployable to aws lambda (See below)

## ASP.NET Core Minimal API Serverless Application

This project shows how to run an ASP.NET Core Web API project as an AWS Lambda exposed through Amazon API Gateway. The NuGet package [Amazon.Lambda.AspNetCoreServer](https://www.nuget.org/packages/Amazon.Lambda.AspNetCoreServer) contains a Lambda function that is used to translate requests from API Gateway into the ASP.NET Core framework and then the responses from ASP.NET Core back to API Gateway.


For more information about how the Amazon.Lambda.AspNetCoreServer package works and how to extend its behavior view its [README](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.AspNetCoreServer/README.md) file in GitHub.

## Executable Assembly

.NET Lambda projects that use C# top level statements like this project must be deployed as an executable assembly instead of a class library. To indicate to Lambda that the .NET function is an executable assembly the
Lambda function handler value is set to the .NET Assembly name. This is different then deploying as a class library where the function handler string includes the assembly, type and method name.

To deploy as an executable assembly the Lambda runtime client must be started to listen for incoming events to process. For an ASP.NET Core application the Lambda runtime client is started by included the
`Amazon.Lambda.AspNetCoreServer.Hosting` NuGet package and calling `AddAWSLambdaHosting(LambdaEventSource.HttpApi)` passing in the event source while configuring the services of the application. The
event source can be API Gateway REST API and HTTP API or Application Load Balancer.

### Project Files

* serverless.template - an AWS CloudFormation Serverless Application Model template file for declaring your Serverless functions and other AWS resources
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
* Program.cs - entry point to the application that contains all of the top level statements initializing the ASP.NET Core application.
The call to `AddAWSLambdaHosting` configures the application to work in Lambda when it detects Lambda is the executing environment.
* Controllers\CalculatorController - example Web API controller

You may also have a test project depending on the options selected.

## Here are some steps to follow from Visual Studio

To deploy your Serverless application, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed application open the Stack View window by double-clicking the stack name shown beneath the AWS CloudFormation node in the AWS Explorer tree. The Stack View also displays the root URL to your published application.

## Here are some steps to follow to get started from the command line

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.

```bash
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.

```bash
    dotnet tool update -g Amazon.Lambda.Tools
```

Deploy application

```bash
    cd "github-dispatcher/src/github-dispatcher"
    dotnet lambda deploy-serverless
```

