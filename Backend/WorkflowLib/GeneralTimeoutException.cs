namespace WorkflowLib;

public class GeneralTimeoutException : Exception
{
    public GeneralTimeoutException () : base("Timeout has occurred")
    {}
}