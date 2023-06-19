using hackaton.Models;
using hackaton.Models.Caches;
using hackaton.Models.DAO;
using hackaton.Models.Injectors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace hackaton.Controllers
{
    public class ClientController : Controller
    {
        private readonly UserCacheService _userService;
        private readonly Context _context;
      
        public ClientController(UserCacheService cache, Context context) { 
            _userService = cache;
            _context = context;
        }

        // GET: ClientController
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        public ActionResult Index()
        {
            string cpf = HttpContext.Session.GetString("CPF");
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            User user = _userService.GetUserByCPFAsync(cpf);
            Console.WriteLine(user.IsAdmin);
            if (user != null && user.Id == userId && user.IsAdmin) {
                return RedirectToAction("Index", "Users");
            }
            //ViewBag.User = user;
            
            return View(user);
        }

        // GET: ClientController/Logout
        public ActionResult Logout()
        {
            //limpa a session do client
            HttpContext.Session.Clear();
            //Deleta o cookie que tem os dados de sessão
            Response.Cookies.Delete("MyAuthCookie");

            //Direciona o usuario a pagina /Home/Index
            return RedirectToAction("Index","Home");
        }

        // GET, recebe o User da página em forma de JSON, remonta o User, e usa o ApiRequest para solicitar a deleção ao Back
        public async Task<IActionResult> DeleteClient([FromBody] JObject userData)
        {
            try
            {
                User user = userData.ToObject<User>();

                user = await ApiRequest.deleteUser(user);
                if (User != null)
                {
                    return RedirectToAction("Logout");
                }
                else
                {   // Aqui a API retornou um erro, por algum motivo
                    return StatusCode(500, "Algo deu errado.");
                }
            }
            catch (Exception ex)    // Provavelmente só acontece caso chegue um userData inválido para conversão
            {
                return StatusCode(500, ex.Message);
            }
        }

        public async Task<IActionResult> EditClient(User user)
        {
            var result = await ApiRequest.modifyUserLogged(user);
            if (result != null)
            {
                return View("~/Views/Client/Index.cshtml",result);
            }
            return StatusCode(500, "O servidor não foi capaz de editar o usuário fornecido.");
        }
    }
}
