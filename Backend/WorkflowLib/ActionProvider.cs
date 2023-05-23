using Microsoft.AspNetCore.SignalR;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using WorkflowApi.Hubs;

namespace WorkflowLib;

public class ActionProvider : IWorkflowActionProvider
{
    private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>
        _asyncActions = new();
    
    private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, bool>>
        _syncConditions = new();

    private IHubContext<ProcessConsoleHub> _processConsoleHub;

    public ActionProvider(IHubContext<ProcessConsoleHub> processConsoleHub)
    {
        _processConsoleHub = processConsoleHub;
        _asyncActions.Add(nameof(SendMessageToProcessConsoleAsync), SendMessageToProcessConsoleAsync);
        _syncConditions.Add(nameof(IsHighPriority),IsHighPriority);
        _syncConditions.Add(nameof(IsMediumPriority), IsMediumPriority);
    }

    public void ExecuteAction(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
        string actionParameter)
    {
        throw new NotImplementedException();
    }

    public async Task ExecuteActionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
        string actionParameter,
        CancellationToken token)
    {
        if (!_asyncActions.ContainsKey(name))
        {
            throw new NotImplementedException($"Async Action with name {name} isn't implemented");
        }

        await _asyncActions[name].Invoke(processInstance, runtime, actionParameter, token);
    }

    public bool ExecuteCondition(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
        string actionParameter)
    {
        if (!_syncConditions.ContainsKey(name))
        {
            throw new NotImplementedException($"Sync Condition with name {name} isn't implemented");
        }

        return _syncConditions[name].Invoke(processInstance, runtime, actionParameter);
    }

    public Task<bool> ExecuteConditionAsync(string name, ProcessInstance processInstance, WorkflowRuntime runtime,
        string actionParameter, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public bool IsActionAsync(string name, string schemeCode)
    {
        return _asyncActions.ContainsKey(name);
    }

    public bool IsConditionAsync(string name, string schemeCode)
    {
        return false; //we have no async conditions now
    }

    public List<string> GetActions(string schemeCode, NamesSearchType namesSearchType)
    {
        return _asyncActions.Keys.ToList();
    }

    public List<string> GetConditions(string schemeCode, NamesSearchType namesSearchType)
    {
        return _syncConditions.Keys.ToList();
    }

    //it is internal just to have possibility to use nameof()  
    internal async Task SendMessageToProcessConsoleAsync(ProcessInstance processInstance, WorkflowRuntime runtime,
        string actionParameter, CancellationToken token)
    {
        await _processConsoleHub.Clients.All.SendAsync("ReceiveMessage", new
        {
            processId = processInstance.ProcessId,
            message = actionParameter
        }, cancellationToken: token);
    }

    private bool IsHighPriority(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
    {
        dynamic report = processInstance.GetParameter<DynamicParameter>("Report");
        return (report.CompanySize == "Big" && report.Position == "Management") 
               || (report.CompanySize == "Small" && report.Position == "Development");
    }
    
    private bool IsMediumPriority(ProcessInstance processInstance, WorkflowRuntime runtime, string actionParameter)
    {
        dynamic report = processInstance.GetParameter<DynamicParameter>("Report");
        return report.Position != "Other" && !IsHighPriority(processInstance, runtime, actionParameter);
    }
}