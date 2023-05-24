using System.Xml.Linq;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Plugins;

namespace WorkflowLib;

public class WorkflowRuntimeLocator
{
    public WorkflowRuntime Runtime { get; private set; }

    public WorkflowRuntimeLocator(MsSqlProviderLocator workflowProviderLocator,
        IWorkflowActionProvider actionProvider, IWorkflowRuleProvider ruleProvider,
        IDesignerParameterFormatProvider designerParameterFormatProvider)

    {
        // TODO If you have a license key, you have to register it here
        // WorkflowRuntime.RegisterLicense(licenseKey);

        var builder = new WorkflowBuilder<XElement>(
            workflowProviderLocator.Provider,
            new XmlWorkflowParser(),
            workflowProviderLocator.Provider
        ).WithDefaultCache();

        // we need BasicPlugin to send email
        var basicPlugin = new BasicPlugin
        {
            Setting_MailserverFrom = "mail@gmail.com",
            Setting_Mailserver = "smtp.gmail.com",
            Setting_MailserverSsl = true,
            Setting_MailserverPort = 587,
            Setting_MailserverLogin = "mail@gmail.com",
            Setting_MailserverPassword = "Password"
        };
        var runtime = new WorkflowRuntime()
            .WithPlugin(basicPlugin)
            .WithBuilder(builder)
            .WithPersistenceProvider(workflowProviderLocator.Provider)
            .EnableCodeActions()
            .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
            // add custom activity
            .WithCustomActivities(new List<ActivityBase> {new WeatherActivity()})
            // add custom rule provider
            .WithRuleProvider(ruleProvider)
            .WithActionProvider(actionProvider)
            .WithDesignerParameterFormatProvider(designerParameterFormatProvider)
            .RegisterAssemblyForCodeActions(typeof(WorkflowRuntimeLocator).Assembly)
            .AsSingleServer();

        // events subscription
        runtime.OnProcessActivityChangedAsync += (sender, args, token) => Task.CompletedTask;
        runtime.OnProcessStatusChangedAsync += (sender, args, token) => Task.CompletedTask;

        runtime.OnWorkflowErrorAsync += async (sender, args, token) =>
        {
            var processInstance = args.ProcessInstance;
            var generalErrorActivity = processInstance.ProcessScheme.Activities
                .FirstOrDefault(a => a.State == "OnGeneralError" && a.IsForSetState);
            if (generalErrorActivity == null) return;
            args.SuppressThrow = true;
            await runtime.SetActivityWithExecutionAsync(processInstance.IdentityId,
                processInstance.ImpersonatedIdentityId,
                new Dictionary<string, object>(), generalErrorActivity, processInstance, true, token);
        };

        Runtime = runtime;
    }

}