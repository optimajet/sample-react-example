using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace WorkflowLib;

public class DesignerParameterFormatProvider : IDesignerParameterFormatProvider
{
    public List<CodeActionParameterDefinition> GetFormat(CodeActionType type, string name, string schemeCode)
    {
        if (type == CodeActionType.Action && name == nameof(ActionProvider.SendMessageToProcessConsoleAsync))
        {
            return new List<CodeActionParameterDefinition>()
            {
                new CodeActionParameterDefinition()
                {
                    Type = ParameterType.TextArea,
                    IsRequired = true,
                    Title = "Console message"
                }
            };
        }

        return new List<CodeActionParameterDefinition>();
    }
}