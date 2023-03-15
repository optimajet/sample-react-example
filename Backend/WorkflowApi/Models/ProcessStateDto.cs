using OptimaJet.Workflow.Core.Runtime;

namespace WorkflowApi.Models;

public class ProcessStateDto
{
    public string Name { get; set; }
    public string LocalizedName { get; set; }
    
    public bool IsCurrent { get; set; }
    
    public static ProcessStateDto? FromProcessState(WorkflowState? state, string currentState)
    {
        if (state == null)
        {
            return null;
        }
        
        return new ProcessStateDto
        {
            Name = state.Name,
            LocalizedName = state.VisibleName,
            IsCurrent = state.Name == currentState
        };
    }
}