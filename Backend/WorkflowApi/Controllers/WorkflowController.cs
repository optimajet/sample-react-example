using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Persistence;
using WorkflowApi.Models;
using WorkflowLib;

namespace WorkflowApi.Controllers;

[Route("api/workflow")]
public class WorkflowController : ControllerBase
{
    /// <summary>
    /// Returns process schemes from the database
    /// </summary>
    /// <returns>Process schemes</returns>
    [HttpGet]
    [Route("schemes")]
    public async Task<IActionResult> Schemes()
    {
        // getting a connection to the database
        await using var connection = WorkflowInit.Provider.OpenConnection();
        // creating parameters for the "ORDER BY" clause
        var orderParameters = new List<(string parameterName, SortDirection sortDirection)>
        {
            ("Code", SortDirection.Asc)
        };
        // creating parameters for the "LIMIT" and "OFFSET" clauses
        var paging = Paging.Create(0, 200);
        // getting schemes from the database
        var list = await WorkflowInit.Provider.WorkflowScheme
            .SelectAllWorkflowSchemesWithPagingAsync(connection, orderParameters, paging);

        // converting schemes to DTOs
        var results = list.Select(s => new WorkflowSchemeDto
        {
            Code = s.Code,
            Tags = s.Tags
        });
        return Ok(results);
    }

    /// <summary>
    /// Returns process instances from the database
    /// </summary>
    /// <returns>Process instances</returns>
    [HttpGet]
    [Route("instances")]
    public async Task<IActionResult> Instances()
    {
        // getting a connection to the database
        await using var connection = WorkflowInit.Provider.OpenConnection();
        // creating parameters for the "ORDER BY" clause
        var orderParameters = new List<(string parameterName, SortDirection sortDirection)>
        {
            ("CreationDate", SortDirection.Desc)
        };
        // creating parameters for the "LIMIT" and "OFFSET" clauses
        var paging = Paging.Create(0, 200);
        // getting process instances from the database
        var processes = await WorkflowInit.Provider.WorkflowProcessInstance
            .SelectAllWithPagingAsync(connection, orderParameters, paging);

        // converting process instances to DTOs
        var result = processes.Select(async p =>
            {
                // getting process scheme from another table to get SchemeCode
                var schemeEntity = await WorkflowInit.Provider.WorkflowProcessScheme.SelectByKeyAsync(connection, p.SchemeId!);
                // converting process instances to DTO
                return ConvertToWorkflowProcessDto(p, schemeEntity.SchemeCode);
            })
            .Select(t => t.Result)
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Creates a process instance for the process scheme
    /// </summary>
    /// <param name="schemeCode">Process scheme code</param>
    /// <returns>Process instance</returns>
    [HttpGet]
    [Route("createInstance/{schemeCode}")]
    public async Task<IActionResult> CreateInstance(string schemeCode)
    {
        // generating a new processId
        var processId = Guid.NewGuid();
        // creating a new process instance
        await WorkflowInit.Runtime.CreateInstanceAsync(schemeCode, processId);

        // getting a connection to the database
        await using var connection = WorkflowInit.Provider.OpenConnection();
        // getting process instance from the database
        var processInstanceEntity = await WorkflowInit.Provider.WorkflowProcessInstance
            .SelectByKeyAsync(connection, processId);

        // converting process instances to DTO
        var workflowProcessDto = ConvertToWorkflowProcessDto(processInstanceEntity, schemeCode);
        return Ok(workflowProcessDto);
    }

    /// <summary>
    /// Returns process instance commands
    /// </summary>
    /// <param name="processId">Unique process identifier</param>
    /// <param name="identityId">Command executor identifier</param>
    /// <returns></returns>
    [HttpGet]
    [Route("commands/{processId:guid}/{identityId}")]
    public async Task<IActionResult> Commands(Guid processId, string identityId)
    {
        // getting a process instance and its parameters
        var process = await WorkflowInit.Runtime.GetProcessInstanceAndFillProcessParametersAsync(processId);
        // getting available commands for a process instance
        var commands = await WorkflowInit.Runtime.GetAvailableCommandsAsync(processId, identityId);
        // convert process instance commands to a list of strings
        var commandNames = commands?.Select(c => c.CommandName).ToList() ?? new List<string>();
        // creating the resulting DTO
        var dto = new WorkflowProcessCommandsDto
        {
            Id = process.ProcessId.ToString(),
            Commands = commandNames
        };
        return Ok(dto);
    }

    /// <summary>
    /// Executes a command on a process instance
    /// </summary>
    /// <param name="processId">Unique process identifier</param>
    /// <param name="command">Command</param>
    /// <param name="identityId">Command executor identifier</param>
    /// <returns>true if the command was executed, false otherwise</returns>
    [HttpGet]
    [Route("executeCommand/{processId:guid}/{command}/{identityId}")]
    public async Task<IActionResult> ExecuteCommand(Guid processId, string command, string identityId)
    {
        // getting available commands for a process instance
        var commands = await WorkflowInit.Runtime.GetAvailableCommandsAsync(processId, identityId);
        // search for the necessary command
        var workflowCommand = commands?.First(c => c.CommandName == command)
                              ?? throw new ArgumentException($"Command {command} not found");
        // executing the command
        var result = await WorkflowInit.Runtime.ExecuteCommandAsync(workflowCommand, identityId, null);
        return Ok(result.WasExecuted);
    }

    /// <summary>
    /// Converts ProcessInstanceEntity to WorkflowProcessDto
    /// </summary>
    /// <param name="processInstanceEntity">Process instance entity</param>
    /// <param name="schemeCode">Scheme code</param>
    /// <returns>WorkflowProcessDto</returns>
    private static WorkflowProcessDto ConvertToWorkflowProcessDto(ProcessInstanceEntity processInstanceEntity, string schemeCode)
    {
        var workflowProcessDto = new WorkflowProcessDto
        {
            Id = processInstanceEntity.Id.ToString(),
            Scheme = schemeCode,
            StateName = processInstanceEntity.StateName,
            ActivityName = processInstanceEntity.ActivityName,
            CreationDate = processInstanceEntity.CreationDate.ToString(CultureInfo.InvariantCulture)
        };
        return workflowProcessDto;
    }
}