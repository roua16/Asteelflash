using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.Controllers
{
    [ApiController]
    [Route("api/dev")]
    public class DevAuthController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public DevAuthController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            // Only expose in Development to avoid leaking environment info
            if (!_env.IsDevelopment())
                return NotFound();

            var user = HttpContext.User;
            return Ok(new
            {
                IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
                AuthenticationType = user?.Identity?.AuthenticationType,
                Name = user?.Identity?.Name,
                Claims = user?.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-check")]
        public IActionResult AdminCheck()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            return Ok(new { Message = "You are Admin", Name = HttpContext.User.Identity?.Name });
        }
    }
}