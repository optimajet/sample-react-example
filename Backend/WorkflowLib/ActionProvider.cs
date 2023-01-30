using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using WorkflowApi.Hubs;

namespace WorkflowLib;

public class ActionProvider : IWorkflowActionProvider
{
    private readonly Dictionary<string, Func<ProcessInstance, WorkflowRuntime, string, CancellationToken, Task>>
        _asyncActions = new();

    private IHubContext<ProcessConsoleHub> _processConsoleHub;

    public ActionProvider(IHubContext<ProcessConsoleHub> processConsoleHub)
    {
        _processConsoleHub = processConsoleHub;
        _asyncActions.Add(nameof(SendMessageToProcessConsoleAsync), SendMessageToProcessConsoleAsync);
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public List<string> GetActions(string schemeCode, NamesSearchType namesSearchType)
    {
        return _asyncActions.Keys.ToList();
    }

    public List<string> GetConditions(string schemeCode, NamesSearchType namesSearchType)
    {
        return new List<string>();
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
}