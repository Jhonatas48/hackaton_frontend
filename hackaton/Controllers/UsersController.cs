using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using hackaton.Models;
using hackaton.Models.DAO;
using DevOne.Security.Cryptography.BCrypt;
using hackaton.Models.Caches;
using hackaton.Models.Validations;
using hackaton.Models.Injectors;
using Microsoft.IdentityModel.Tokens;

namespace hackaton.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserCacheService _userCacheService;
        public UsersController(UserCacheService cache)
        {
            _userCacheService = cache;
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

        public IActionResult AllowedRegister(string cpf)
        {
            Console.WriteLine(cpf);

            if (_userCacheService.GetUserByCPFAsync(cpf) == null)
            {
                return Json(true);
            }

            return Json("CPF já cadastrado");

        }

        // HOTFIX: Usar ApiRequest
        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        // GET: Users
        public async Task<IActionResult> Index()
        {
            var ListaUsers = await ApiRequest.getUsers();
            return (ListaUsers != null) ?
                View("~/Views/Admin/Index.cshtml", ListaUsers.Where(user => user.Active == true).ToList()) :
                Problem("Entity set 'ListaUsers'  is null.");
            //return _context.Users != null ?
            //            View("~/Views/Admin/Index.cshtml", _context.Users.Where(user => user.Active == true).ToList()) :
            //            Problem("Entity set 'Context.Users'  is null.");
            //return View("~/Views/Admin/Index.cshtml");
        }

        // GET: Users/Create
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        public IActionResult Create()
        {
            return View();
        }

        // HOTFIX: Usar ApiRequest
        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Password,CPF,IsAdmin")] User user)
        {
            if (ModelState.IsValid)
            {
                //var cpfExists = await _context.Users.AnyAsync(u => u.CPF == user.CPF);
                var cpfExists = (await ApiRequest.getUsers()).Any(u => u.CPF == user.CPF);
                if (cpfExists)
                {
                    ModelState.AddModelError("CPF", "O CPF já está cadastrado.");
                    return View(user);
                }

                string password = user.Password;
                user.Password = BCryptHelper.HashPassword(password, BCryptHelper.GenerateSalt());
                
                await ApiRequest.createUser(user);
                //_context.Add(user);
                //await _context.SaveChangesAsync();
                
                _userCacheService.AddUserToCache(user);
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // HOTFIX: Usar ApiRequest
        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Password,CPF,IsAdmin")] User user)
        {

            user.Id = id;
            user = await ApiRequest.modifyUserLogged(user);

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
            return View("~/Views/Admin/Edit.cshtml", user);
        }

        // HOTFIX: Usar ApiRequest
        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");

            //if (_context.Users == null)
            var ListaUsers = await ApiRequest.getUsers();
            if (ListaUsers == null )
            {
                return Problem("Entity set 'ListaUsers'  is null.");
            }

            //var user = await _context.Users.FindAsync(id);
            var user = ListaUsers.Find(u => u.Id == id);

            if (user.Id == userId)
            {
                ModelState.AddModelError("Name", "Você não pode excluir a si mesmo");
                return RedirectToAction("Index");
            }

            if (user != null)
            {
                user.Active = false;

                await ApiRequest.deleteUser(user);
                //_context.Users.Update(user);
                //await _context.SaveChangesAsync();
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
