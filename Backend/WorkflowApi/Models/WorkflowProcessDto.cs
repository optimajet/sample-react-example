namespace WorkflowApi.Models;

public class WorkflowProcessDto
{
    public string Id { get; set; }
    public string Scheme { get; set; }
    public string StateName { get; set; }
    public string ActivityName { get; set; }
    public string CreationDate { get; set; }
}