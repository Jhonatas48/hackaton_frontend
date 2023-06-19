using DevOne.Security.Cryptography.BCrypt;
using hackaton.Models;
using hackaton.Models.Caches;
using hackaton.Models.DAO;
using hackaton.Models.Injectors;
using hackaton.Models.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace hackaton.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<RedirectClient> _redirectClient;

        private UserCacheService _userCacheService;
        public HomeController(ILogger<HomeController> logger,Context context,UserCacheService cache)
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
            // HOTFIX: Usar ApiRequest.GetUsers()
            //var userRetrieve = _context.Users.FirstOrDefault(u => u.CPF == user.CPF);// _userCacheService.GetUserByCPFAsync(user.CPF);//_context.Users.FirstOrDefault(u => u.CPF.Equals(user.CPF));

            var users = await ApiRequest.getUsers();
            var userRetrieve = users.FirstOrDefault(u => u.CPF == user.CPF);

            //Usuario não existe ou credenciais estão inválidas
            if (userRetrieve == null || !BCryptHelper.CheckPassword(user.Password, userRetrieve.Password))
            { 
                return false;
            }

           
            return true;
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
            user = _userCacheService.GetUserByCPFAsync(user.CPF);
            HttpContext.Session.SetString("SessionId", user.CPF);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("CPF", user.CPF);
          
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
            // HOTFIX: Usar ApiRequest.GetUsers()
            //var cpfExists =  _context.Users.Any(u => u.CPF == user.CPF);
            var cpfExists = (await ApiRequest.getUsers()).Any(u => u.CPF == user.CPF);

            if (cpfExists)
            {
                ModelState.AddModelError("CPF", "O CPF já está cadastrado.");
                return View(user);
            }
            

            user.Password = BCryptHelper.HashPassword(user.Password,BCryptHelper.GenerateSalt());

            // HOTFIX: Usar ApiRequest.createUser()
            //_context.Users.Add(user);
            //_context.SaveChanges();
            user = await ApiRequest.createUser(user);
            if (user == null)
            {
                return StatusCode(500, "O servidor foi incapaz de completar o registro.");
            }

            user = _userCacheService.GetUserByCPFAsync(user.CPF);
            HttpContext.Session.SetString("SessionId", user.CPF);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("CPF", user.CPF);
            return View("SucessRegister",user);
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