namespace WorkflowApi.Models;

public class ProcessActivityDto
{
    public string Name { get; set; }
    
    public bool IsCurrent { get; set; }
    
    public ProcessStateDto? State { get; set; }
}