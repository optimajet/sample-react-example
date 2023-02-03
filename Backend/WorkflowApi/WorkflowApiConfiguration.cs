namespace WorkflowApi;

public class WorkflowApiConfiguration
{
    public WorkflowApiCorsConfiguration Cors { get; set; }
}

public class WorkflowApiCorsConfiguration
{
    public List<string> Origins { get; set; }
}