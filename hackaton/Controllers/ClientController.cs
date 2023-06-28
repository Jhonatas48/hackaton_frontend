using frontend_hackaton.Models.Desserializers;
using hackaton.Models;
using hackaton.Models.Caches;
using hackaton.Models.Injectors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace hackaton.Controllers
{
    public class ClientController : Controller
    {
        
      
        public ClientController() { 
           
        }

        // GET: ClientController
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        public async Task<ActionResult> Index()
        {
            string cpf = HttpContext.Session.GetString("CPF");
            int userId = (int)HttpContext.Session.GetInt32("UserId");

            var users = await ApiRequest.getUsers(cpf);
            User user = users.FirstOrDefault();
           
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
        public async Task<IActionResult> Delete(User user)//([FromBody] JObject userData)
        {
            try
            {
                //User user = userData.ToObject<User>();

                user = await ApiRequest.deleteUserLogged(user);
                if (User == null)
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

        public async Task<IActionResult> Edit(User user)
        {
            ApiResponse<User> response = await ApiRequest.modifyUserLogged(user);

            if (response == null)
            {
                return StatusCode(500, "O servidor não foi capaz de editar o usuário fornecido.");
            }
            if (response.Sucess)
            {
                return View("~/Views/Client/Index.cshtml", response.classObject);
            }
            if(response.statusCode == 404)
            {
                return NotFound();
            }
            foreach (var error in response.Errors)
            {
                string campo = error.Key;
                List<string> mensagensErro = error.Value;

                foreach (var mensagemErro in mensagensErro)
                {
                    ModelState.AddModelError(campo, mensagemErro);
                }
            }

            return View("~/Views/Client/Index.cshtml", user);
        }
    }
}
