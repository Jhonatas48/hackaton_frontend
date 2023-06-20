
using Microsoft.AspNetCore.Mvc;
using hackaton.Models;
using hackaton.Models.Caches;
using hackaton.Models.Injectors;
using Microsoft.IdentityModel.Tokens;
using frontend_hackaton.Models.Desserializers;

namespace hackaton.Controllers
{
    public class UsersController : Controller
    {
       // private readonly UserCacheService _userCacheService;
        public UsersController(UserCacheService cache)
        {
           // _userCacheService = cache;
        }

        // HOTFIX: Usar ApiRequest
        public async Task<IActionResult> Search(string searchQuery)
        {
            List<User> ListaUsers;
            ListaUsers = await ApiRequest.getUsers();

            if (searchQuery.IsNullOrEmpty())
            {
                //ListaUsers = _context.Users.Where(u => u.Active == true).ToList();
                ListaUsers = ListaUsers.Where(u => u.Active == true).ToList();
            }
            else
            {
                //ListaUsers = _context.Users.Where(u => (u.Active == true) && ((u.CPF.Contains(searchQuery)) || (u.Name.Contains(searchQuery)))).OrderBy(u => u.Name).ToList();
                ListaUsers = ListaUsers.Where(u => (u.Active == true) && ((u.CPF.Contains(searchQuery)) || (u.Name.Contains(searchQuery)))).OrderBy(u => u.Name).ToList();
            }

            return View("~/Views/Admin/Index.cshtml", ListaUsers);
        }
        
        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        // GET: Users
        public async Task<IActionResult> Index()
        {
            var ListaUsers = await ApiRequest.getUsers();
            return (ListaUsers != null) ?
                View("~/Views/Admin/Index.cshtml", ListaUsers.Where(user => user.Active == true).ToList()) :
                Problem("Entity set 'ListaUsers'  is null.");
          
        }

        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        public async Task<IActionResult> Edit(int? id)
        {
            string cpf = HttpContext.Session.GetString("CPF");
           var users = await ApiRequest.getUsers(cpf);
            User userAdmin = users.FirstOrDefault();
            User user = await ApiRequest.getUserToModify(id,userAdmin);
            if(user == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/Edit.cshtml", user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Password,CPF,IsAdmin")] User user)
        {

            user.Id = id;
            ApiResponse<User> response =  await ApiRequest.modifyUser(user);

            if (!response.Sucess)
            {
                foreach (var error in response.Errors)
                {
                    string campo = error.Key;
                    List<string> mensagensErro = error.Value;

                    foreach (var mensagemErro in mensagensErro)
                    {
                        ModelState.AddModelError(campo, mensagemErro);
                    }
                }
                Console.WriteLine(ModelState.IsValid);
                ModelState.Remove("Agendamentos");
                 return View("~/Views/Admin/Edit.cshtml", user);
            }
            //var userRetrieve = (await ApiRequest.getUsers()).Where(user => user.Id == userId).Single();
            ////var userRetrieve = _context.Users.Where(u => u.Id == userId).Single();
            //user.IsAdmin = userRetrieve.IsAdmin;
            //user.Id = userId;

            //ModelState.Remove("user.QrCodes");
            //ModelState.Remove("user.Agendamentos");
            //ModelState.Remove("user.Properties");
            //ModelState.Remove("user.CPF");  //suspeito que alguma verificação aqui esteja quebrada, se tiver a validação do "já cadastrado"

            //if (ModelState.IsValid)
            //{
            //    try
            //    {
            //        string password = user.Password;
            //        user.Password = BCryptHelper.HashPassword(password, BCryptHelper.GenerateSalt());

            //        _context.ChangeTracker.Clear();

            //        _context.Update(user);
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!UserExists(user.Id))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction("Index");
            //}
            return RedirectToAction("Index");
        }


        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        public async Task<IActionResult> Delete(int? id)
        {
            string cpf = HttpContext.Session.GetString("CPF");
            var users = await ApiRequest.getUsers(cpf);
            User userAdmin = users.FirstOrDefault();
            User user = await ApiRequest.getUserToModify(id, userAdmin);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/Edit.cshtml", user);
        }


        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
       // [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string cpf = HttpContext.Session.GetString("CPF");
            var users = await ApiRequest.getUsers(cpf);
            User userAdmin = users.FirstOrDefault();

            int userId = (int)HttpContext.Session.GetInt32("UserId");

            var user = await ApiRequest.getUserToModify(id,userAdmin);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == userId)
            {
                ModelState.AddModelError("Name", "Você não pode excluir a si mesmo");
                return RedirectToAction("Index");
            }
        
           var userDeleted = await ApiRequest.deleteUser(id,userAdmin);

            if (userDeleted == null)
            {
                return Forbid();
            }
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UserExists(int id)
        {
            return ((await ApiRequest.getUsers())?.Any(u => u.Id == id)).GetValueOrDefault();
            //return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
