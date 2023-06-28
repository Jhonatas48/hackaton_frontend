using frontend_hackaton.Models.Desserializers;
using hackaton.Models;
using hackaton.Models.Caches;
using hackaton.Models.DAO;
using hackaton.Models.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace hackaton.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<RedirectClient> _redirectClient;

        private UserCacheService _userCacheService;
        public HomeController(ILogger<HomeController> logger,UserCacheService cache)
        {
            _logger = logger;
            _userCacheService = cache;
           
        }
      
        public IActionResult Index()
        {
            var session = HttpContext.Session;
            
            if (!string.IsNullOrEmpty(session.GetString("CPF")) && !string.IsNullOrEmpty(session.GetString("SessionId")) &&  session.GetInt32("UserId") != null ) {
                 
                return RedirectToAction("Index", "Client");
            }

            return View();
        }

        public IActionResult Login() {

            return View("Login");
        }

        public async Task<bool> validateLogin(User user) {
           
            return await ApiRequest.requestLogin(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User user)
        {
            if (user == null) {
                return new BadRequestObjectResult(new { message = "User is required" }); ;
            }

            if (!(await validateLogin(user)))
            {
                ModelState.AddModelError("CPF", "CPF ou Senha inválidos");
                ModelState.AddModelError("Password", "CPF ou Senha inválidos");
                return View();
            }
            user = await _userCacheService.GetUserByCPFAsync(user.CPF);
            HttpContext.Session.SetString("SessionId", user.CPF);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("CPF", user.CPF);
            string cpf = HttpContext.Session.GetString("CPF");
            if (user.IsAdmin) {
                return RedirectToAction("Index", "Users");
            }

            return RedirectToAction("Index","Client");
           
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            Console.WriteLine("TESTE");
            var cpfExists = (await ApiRequest.getUsers(""))?.Any(u => u.CPF == user.CPF);

            if (cpfExists != null && cpfExists == true)
            {
                ModelState.AddModelError("CPF", "O CPF já está cadastrado.");
                return View(user);
            }
           
            ApiResponse<User> response = await ApiRequest.createUser(user);

            if (!response.Sucess)
            {
                foreach (var error in response.Errors)
                {
                    string campo = error.Key;
                    List<string> mensagensErro = error.Value;

                    foreach (var mensagemErro in mensagensErro)
                    {
                        Console.WriteLine("c: "+campo + ": " + mensagemErro);
                        ModelState.AddModelError(campo, mensagemErro);
                    }
                }
                Console.WriteLine(ModelState.IsValid);
                ModelState.Remove("Agendamentos");
                return View(user);
            }

            user = await _userCacheService.GetUserByCPFAsync(user.CPF);
            HttpContext.Session.SetString("SessionId", user.CPF);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("CPF", user.CPF);
            //return View("SucessRegister",user);
            return RedirectToAction("Index", "Client");
        }

        public IActionResult SucessRegister() {
            
            return View();
        }

        public IActionResult PermissionDenied() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}