using Microsoft.AspNetCore.Mvc;
using WorkflowLib;

namespace WorkflowApi.Controllers;

[Route("api/user")]
public class UserController : ControllerBase
{
    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(Users.Data);
    }
}