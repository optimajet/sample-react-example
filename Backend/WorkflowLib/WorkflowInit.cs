using System.Xml.Linq;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.DbPersistence;
using OptimaJet.Workflow.Plugins;

namespace WorkflowLib;

public static class WorkflowInit
{
    private const string ConnectionString = "Data Source=(local);Initial Catalog=master;User ID=sa;Password=StrongPassword#1";

    private static readonly Lazy<WorkflowRuntime> LazyRuntime = new(InitWorkflowRuntime);
    private static readonly Lazy<MSSQLProvider> LazyProvider = new(InitMssqlProvider);

    public static WorkflowRuntime Runtime => LazyRuntime.Value;
    public static MSSQLProvider Provider => LazyProvider.Value;

    private static MSSQLProvider InitMssqlProvider()
    {
        return new MSSQLProvider(ConnectionString);
    }

    private static WorkflowRuntime InitWorkflowRuntime()
    {
        //WorkflowRuntime.RegisterLicense(Secrets.LicenseKey);

        var builder = new WorkflowBuilder<XElement>(
            Provider,
            new XmlWorkflowParser(),
            Provider
        ).WithDefaultCache();

        // we need BasicPlugin to send email
        var basicPlugin = new BasicPlugin
        {
            Setting_MailserverFrom = "mail@gmail.com",
            Setting_Mailserver = "smtp.gmail.com",
            Setting_MailserverSsl = true,
            Setting_MailserverPort = 587,
            Setting_MailserverLogin = "mail@gmail.com",
            Setting_MailserverPassword = Secrets.MailPassword
        };
        var runtime = new WorkflowRuntime()
            .WithPlugin(basicPlugin)
            .WithBuilder(builder)
            .WithPersistenceProvider(Provider)
            .EnableCodeActions()
            .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
            // add custom activity
            .WithCustomActivities(new List<ActivityBase> {new WeatherActivity()})
            // add custom rule provider
            .WithRuleProvider(new SimpleRuleProvider())
            .AsSingleServer();

        // events subscription
        runtime.OnProcessActivityChangedAsync += (sender, args, token) => Task.CompletedTask;
        runtime.OnProcessStatusChangedAsync += (sender, args, token) => Task.CompletedTask;

        runtime.Start();

        return runtime;
    }
}
