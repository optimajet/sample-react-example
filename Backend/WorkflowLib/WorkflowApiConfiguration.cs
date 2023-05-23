namespace WorkflowLib;

public class WorkflowApiConfiguration
{
    public WorkflowApiCorsConfiguration Cors { get; set; }
    
    public WorkflowRuntimeConfiguration Runtime { get; set; }

    public string LicenseKey { get; set; }
    
}

public class WorkflowRuntimeConfiguration
{
    public bool DisableMultipleProcessActivityChanged { get; set; }
    
    public bool SkipLastMultipleProcessActivityChanged { get; set; }
}

public class WorkflowApiCorsConfiguration
{
    public List<string> Origins { get; set; }
}