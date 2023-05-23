using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Plugins;

namespace WorkflowLib;

public class WorkflowRuntimeLocator
{
    public WorkflowRuntime Runtime { get; private set; }

    public WorkflowRuntimeLocator(MsSqlProviderLocator workflowProviderLocator,
        IWorkflowActionProvider actionProvider, IWorkflowRuleProvider ruleProvider,
        IDesignerParameterFormatProvider designerParameterFormatProvider, IConfiguration configRoot)

    {
        var configuration = configRoot.Get<WorkflowApiConfiguration>();
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

        var runtimeSettings = new WorkflowRuntimeSettings()
        {
            DisableMultipleProcessActivityChanged = configuration.Runtime.DisableMultipleProcessActivityChanged
        };
            
        var runtime = new WorkflowRuntime()
            .WithPlugin(basicPlugin)
            .WithBuilder(builder)
            .WithPersistenceProvider(workflowProviderLocator.Provider)
            .WithRuntimeSettings(runtimeSettings)
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
        
        bool IsProcessConsoleActionExists (string schemeCode)
        {
           return actionProvider
                .GetActions(schemeCode, NamesSearchType.All)
                .Contains(nameof(ActionProvider.SendMessageToProcessConsoleAsync));
        }

        // events subscription
        runtime.OnProcessActivityChangedAsync += async (sender, args, token) =>
        {
            var eventHandleAllowed = args.ProcessInstance.GetParameter<bool?>("HandleProcessActivityChanged") ?? false;
            if (!eventHandleAllowed)
                return;
            
            if (!IsProcessConsoleActionExists(args.SchemeCode))
                return;

            if (configuration.Runtime.SkipLastMultipleProcessActivityChanged)
            {
                var previousEventArgs =
                    args.ProcessInstance.GetParameter<ProcessActivityChangedEventArgs>("PreviousEventArgs");
                
                args.ProcessInstance.SetParameter("PreviousEventArgs", args);

                if (previousEventArgs != null
                    && previousEventArgs.CurrentActivity == args.CurrentActivity
                    && args.TransitionalProcessWasCompleted
                    && !previousEventArgs.TransitionalProcessWasCompleted)
                {
                    return; //skip event
                }
            }

            var consoleMessage = $@"**ActivityChanged**
{args.PreviousActivity?.Name ?? "_"}->{args.CurrentActivity.Name}
Transition: {args.ExecutedTransition?.Name ?? "_"}
Transitional process completed: {(args.TransitionalProcessWasCompleted ? "Yes" : "No")}";
            
            await actionProvider.ExecuteActionAsync(nameof(ActionProvider.SendMessageToProcessConsoleAsync),
                args.ProcessInstance, runtime, consoleMessage, token);
        };
        
        runtime.OnProcessStatusChangedAsync += async (sender, args, token) =>
        {
            var eventHandleAllowed = args.ProcessInstance.GetParameter<bool?>("HandleProcessStatusChanged") ?? false;
            if (!eventHandleAllowed)
                return;

            if (!IsProcessConsoleActionExists(args.SchemeCode))
                return;

            var consoleMessage = $@"**ProcessStatusChanged**
{args.OldStatus.Name}->{args.NewStatus.Name}";

            await actionProvider.ExecuteActionAsync(nameof(ActionProvider.SendMessageToProcessConsoleAsync),
                args.ProcessInstance, runtime, consoleMessage, token);
        };

        runtime.OnBeforeActivityExecutionAsync += async (sender, args, token) =>
        {
            var eventHandleAllowed = args.ProcessInstance.GetParameter<bool?>("HandleBeforeActivityExecution") ?? false;
            if (!eventHandleAllowed)
                return;

            if (!IsProcessConsoleActionExists(args.SchemeCode))
                return;

            var consoleMessage = $@"**BeforeActivityExecution**
{args.ExecutedActivity.Name}";

            await actionProvider.ExecuteActionAsync(nameof(ActionProvider.SendMessageToProcessConsoleAsync),
                args.ProcessInstance, runtime, consoleMessage, token);
        };

        runtime.OnWorkflowError += (sender, args) =>
        {
            var processInstance = args.ProcessInstance;
            var generalErrorActivity = processInstance.ProcessScheme.Activities
                .FirstOrDefault(a => a.State == "OnGeneralError" && a.IsForSetState);
            if (generalErrorActivity == null) return;
            args.SuppressThrow = true;
            runtime.SetActivityWithExecution(processInstance.IdentityId, processInstance.ImpersonatedIdentityId,
                new Dictionary<string, object>(), generalErrorActivity, processInstance, true);
        };

        Runtime = runtime;
    }

}