using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Runtime;
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
    [HttpPost]
    [Route("createInstance/{schemeCode}")]
    public async Task<IActionResult> CreateInstance(string schemeCode, [FromBody] ProcessParametersDto dto)
    {
        // generating a new processId
        var processId = Guid.NewGuid();
        // creating a new process instance
        var createInstanceParams = new CreateInstanceParams(schemeCode, processId);

        if (dto.ProcessParameters.Count > 0)
        {
            var processScheme = await WorkflowInit.Runtime.Builder.GetProcessSchemeAsync(schemeCode);

            foreach (var processParameter in dto.ProcessParameters)
            {
                var (name, value) =
                    GetParameterNameAndValue(processParameter.Name, processParameter.Value, processScheme);
                if (processParameter.Persist)
                {
                    createInstanceParams.AddPersistentParameter(name, value);
                }
                else
                {
                    createInstanceParams.AddTemporaryParameter(name, value);
                }
            }
        }

        await WorkflowInit.Runtime.CreateInstanceAsync(createInstanceParams);

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
        // creating the resulting DTO
        var dto = new WorkflowProcessCommandsDto
        {
            Id = process.ProcessId.ToString(),
            Commands = commands?.Select(c =>
            {
                return new WorkflowProcessCommandDto()
                {
                    Name = c.CommandName,
                    LocalizedName = c.LocalizedName,
                    CommandParameters = c.Parameters.Select(p => new ProcessParameterDto
                    {
                        Name = p.ParameterName,
                        IsRequired = p.IsRequired,
                        Persist = p.IsPersistent,
                        Value = p.DefaultValue as string ?? (p.DefaultValue == null ? String.Empty : ParametersSerializer.Serialize(p.DefaultValue))
                    }).ToList()
                };
            }).ToList() ?? new List<WorkflowProcessCommandDto>()
        };
        return Ok(dto);
    }

    /// <summary>
    /// Executes a command on a process instance
    /// </summary>
    /// <param name="processId">Unique process identifier</param>
    /// <param name="command">Command</param>
    /// <param name="identityId">Command executor identifier</param>
    /// <param name="dto">Command parameters</param>
    /// <returns>true if the command was executed, false otherwise</returns>
    [HttpPost]
    [Route("executeCommand/{processId:guid}/{command}/{identityId}")]
    public async Task<IActionResult> ExecuteCommand(Guid processId, string command, string identityId,
        [FromBody] ProcessParametersDto dto)
    {
        // getting available commands for a process instance
        var commands = await WorkflowInit.Runtime.GetAvailableCommandsAsync(processId, identityId);
        // search for the necessary command
        var workflowCommand = commands?.First(c => c.CommandName == command)
                              ?? throw new ArgumentException($"Command {command} not found");
        
        if (dto.ProcessParameters.Count > 0)
        {
            var processScheme = await WorkflowInit.Runtime.GetProcessSchemeAsync(processId);

            foreach (var processParameter in dto.ProcessParameters)
            {
                var (name, value) =
                    GetParameterNameAndValue(processParameter.Name, processParameter.Value, processScheme);
                
                workflowCommand.SetParameter(name, value, processParameter.Persist);
            }
        }
        // executing the command
        var result = await WorkflowInit.Runtime.ExecuteCommandAsync(workflowCommand, identityId, null);
        return Ok(result.WasExecuted);
    }

    /// <summary>
    /// Changes process parameters without changing a process state
    /// </summary>
    /// <param name="processId">Unique process identifier</param>
    /// <param name="dto">Process parameters</param>
    /// <returns>true if the process parameters has been changed, false otherwise</returns>
    [HttpPost]
    [Route("setProcessParameters/{processId:guid}")]
    public async Task<IActionResult> SetProcessParameters(Guid processId, [FromBody] ProcessParametersDto dto)
    {
        var result = dto.ProcessParameters.Count > 0;

        var processInstance = await WorkflowInit.Runtime.GetProcessInstanceAndFillProcessParametersAsync(processId);

        foreach (var processParameter in dto.ProcessParameters)
        {
            var (name, value) =
                GetParameterNameAndValue(processParameter.Name, processParameter.Value,
                    processInstance.ProcessScheme);

            await processInstance.SetParameterAsync(name, value,
                processParameter.Persist ? ParameterPurpose.Persistence : ParameterPurpose.Temporary);
        }

        foreach (var processParameter in processInstance.ProcessParameters.Where(p =>
                     p.Purpose != ParameterPurpose.System && !dto.ProcessParameters.Any(d => d.Name.Equals(p.Name))))
        {
            processInstance.RemoveParameter(processParameter.Name);
        }

        await processInstance.SaveAsync(WorkflowInit.Runtime);
        return Ok(result);
    }

    /// <summary>
    /// Returns list of scheme parameters and theirs default values 
    /// </summary>
    /// <param name="schemeCode">Process scheme code</param>
    /// <returns></returns>
    [HttpGet]
    [Route("schemeParameters/{schemeCode}")]
    public async Task<IActionResult> SchemeParameters(string schemeCode)
    {
        var processScheme = await WorkflowInit.Runtime.Builder.GetProcessSchemeAsync(schemeCode);

        var processParameterDtos = processScheme.Parameters
            .Where(p => p.Purpose != ParameterPurpose.System)
            .Select(p => new ProcessParameterDto()
            {
                Name = p.Name, Value = p.InitialValue ?? string.Empty, Persist = p.Purpose == ParameterPurpose.Persistence
            }).ToList();
       
        return Ok(processParameterDtos);
    }

    /// <summary>
    /// Returns list of process parameters and theirs values 
    /// </summary>
    /// <param name="processId">Unique process identifier</param>
    /// <returns></returns>
    [HttpGet]
    [Route("schemeParameters/{processId:guid}")]
    public async Task<IActionResult> ProcessParameters(Guid processId)
    {
        var processInstance = await WorkflowInit.Runtime.GetProcessInstanceAndFillProcessParametersAsync(processId);

        var processParameterDtos = processInstance.ProcessParameters
            .Where(p => p.Purpose != ParameterPurpose.System)
            .Select(p => new ProcessParameterDto()
            {
                Name = p.Name,
                Value = SerializeProcessParameter(p),
                Persist = p.Purpose == ParameterPurpose.Persistence
            }).ToList();

        return Ok(processParameterDtos);
    }
    
    /// <summary>
    /// Serializes process parameters in unified way
    /// </summary>
    /// <param name="processParameter">Process parameter with value</param>
    /// <returns></returns>
    private static string SerializeProcessParameter(ParameterDefinitionWithValue processParameter)
    {
        //Implicit parameters are saved as strings
        //Explicit parameters are already deserialized
        var parameterValue = processParameter.IsImplicit
            ? ParametersSerializer.Deserialize<object>(processParameter.Value.ToString())
            : processParameter.Value;
       
        return parameterValue as string ?? ParametersSerializer.Serialize(parameterValue);
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
    
    private static (string Name, object Value) GetParameterNameAndValue(string parameterName, string parameterValue,
        ProcessDefinition scheme)
    {
        var parameter =
            scheme.Parameters.FirstOrDefault(p => p.Name.Equals(parameterName, StringComparison.Ordinal)) ??
            scheme.Parameters.FirstOrDefault(p => p.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase));

        switch (parameter)
        {
            case null:
            {
                var value = DynamicParameter.ParseJson(parameterValue);
                return (parameterName, value);
            }
            case {} when parameter.Type == typeof (string):
            {
                return (parameterName, parameterValue);
            }
            case { Type: { } }:
            {
                return (parameter.Name, ParametersSerializer.Deserialize(parameterValue, parameter.Type));
            }
            default:
            {
                throw new InvalidOperationException();
            }
        }
    }
}