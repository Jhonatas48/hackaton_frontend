using hackaton.Models.DAO;
using hackaton.Models.WebSocket;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using hackaton.Models.Security;
using hackaton.Models.Injectors;
using hackaton.Models;
using Microsoft.EntityFrameworkCore;

namespace hackaton.Controllers
{
    public class WebHookController : Controller
    {

        private readonly IHubContext<RedirectClient> _redirectClient;
        public WebHookController(IHubContext<RedirectClient> redirectClient)
        {

            _redirectClient = redirectClient;

        }

        [BearerAuthorize]
        [HttpPost]
        public async Task<IActionResult> redirectClient(int userId)
        {

            if(userId == 0)
            {
                return BadRequest("Invalid user ID");
            }

            await _redirectClient.Clients.Group("pc_user" +userId).SendAsync("redirect", "/Client/Index");
            ApiRequest.getUsers();
            return Ok();
        }

       
    }
}
