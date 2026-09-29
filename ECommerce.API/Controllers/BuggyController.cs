using ECommerce.Core.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BuggyController : ControllerBase
{
    [HttpGet("notfound")]
    public IActionResult GetNotFoundRequest()
    {
        return NotFound(new ApiResponse(404));
    }

    [HttpGet("servererror")]
    public IActionResult GetServerError()
    {
        throw new Exception("This is a test server exception!");
    }

    [HttpGet("badrequest")]
    public IActionResult GetBadRequest()
    {
        return BadRequest(new ApiResponse(400));
    }

    [HttpGet("badrequest/{id}")]
    public IActionResult GetBadRequest(int id)
    {
        return Ok();
    }
}
