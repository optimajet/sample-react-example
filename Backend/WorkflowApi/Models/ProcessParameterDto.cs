namespace WorkflowApi.Models;

public class ProcessParameterDto
{
    public string Name { get; set; }

    public string Value { get; set; }
    
    public bool Persist { get; set; }
    
    public bool IsRequired { get; set; }
}