using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace WorkflowLib;

public class SimpleRuleProvider : IWorkflowRuleProvider
{
    // name of our rule
    private const string RuleCheckRole = "CheckRole";

    public List<string> GetRules(string schemeCode, NamesSearchType namesSearchType)
    {
        return new List<string> {RuleCheckRole};
    }

    public bool Check(ProcessInstance processInstance, WorkflowRuntime runtime,
        string identityId, string ruleName, string parameter)
    {
        // we check that the identityId satisfies our rule, that is, the user has the role specified in the parameter

        if (RuleCheckRole != ruleName || identityId == null 
                                      || !Users.UserDict.ContainsKey(identityId)) return false;

        var user = Users.UserDict[identityId];
        return user.Roles.Contains(parameter);
    }

    public async Task<bool> CheckAsync(ProcessInstance processInstance, WorkflowRuntime runtime,
        string identityId, string ruleName,
        string parameter,
        CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetIdentities(ProcessInstance processInstance, 
        WorkflowRuntime runtime, string ruleName, string parameter)
    {
        // return all identities (the identity is user name)
        return Users.Data.Select(u => u.Name);
    }

    public async Task<IEnumerable<string>> GetIdentitiesAsync(ProcessInstance processInstance,
        WorkflowRuntime runtime, string ruleName,
        string parameter,
        CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public bool IsCheckAsync(string ruleName, string schemeCode)
    {
        // use the Check method instead of CheckAsync
        return false;
    }

    public bool IsGetIdentitiesAsync(string ruleName, string schemeCode)
    {
        // use the GetIdentities method instead of GetIdentitiesAsync
        return false;
    }
}