namespace WorkflowLib;

public class GeneralAccessDeniedException : Exception
{
    public GeneralAccessDeniedException() : base("Access denied.")
    {}
}