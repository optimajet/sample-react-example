namespace WorkflowApi.Models;

public class WorkflowProcessCommandDto
{
    public string Name { get; set; }
    
    public string LocalizedName { get; set; }
    
    public List<ProcessParameterDto> CommandParameters { get; set; }
}