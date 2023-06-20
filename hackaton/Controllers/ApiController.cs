using Microsoft.AspNetCore.Mvc;
using hackaton.Models.Security;
using hackaton.Models;
using hackaton.Models.WebSocket;
using Microsoft.AspNetCore.SignalR;
using frontend_hackaton.Models.Desserializers;

namespace hackaton.Controllers
{
    public class ApiController : Controller
    {

        private readonly HomeController _homeController;
      
        private readonly IHubContext<RedirectClient> _redirectClient;
        public ApiController(HomeController homeController, IHubContext<RedirectClient> redirectClient)
       {
         _homeController = homeController;
       //  _context = context;
         _redirectClient = redirectClient;

        }
       // GET: ApiController/QRCode
        [BearerAuthorize]
        public async Task<ActionResult> QRCodeAsync([FromBody] QrCode qrCode)
        {
            ApiResponse<QrCode> response = await ApiRequest.validateQrCode(qrCode);
            if(response == null)
            {
                return Problem("Error in request validation the qrCode");
            }
            if (!response.Sucess)
            {
                return StatusCode(response.statusCode, response.Message);
            }

            return Json(response.classObject);
        }

        // GET: ApiController/validateuser
        [BearerAuthorize]
        public async Task<ActionResult> validadeUser([FromBody] User user)
        {
            if(user == null)
            {
                return new BadRequestObjectResult(new { message = "User is required" });
            }

              ModelState.Remove("Name");

            if (!ModelState.IsValid)
            {
                var erros = ModelState.Keys
                 .Where(key => ModelState[key].Errors.Any())
                 .ToDictionary(key => key, key => ModelState[key].Errors.Select(error => error.ErrorMessage).ToList());

                var response = new
                {
                    Message = "Houve erros de validação.",
                    Errors = erros,

                };
                return BadRequest(response);
            }
            if (await _homeController.validateLogin(user))
            {
                return Ok(user);
            }

            return new UnauthorizedObjectResult(new { message = "Invalid Credentials" });
        }

    }
}
