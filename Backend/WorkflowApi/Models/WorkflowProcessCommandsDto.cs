namespace WorkflowApi.Models;

public class WorkflowProcessCommandsDto
{
    public string Id { get; set; }
    public List<WorkflowProcessCommandDto> Commands { get; set; }
}