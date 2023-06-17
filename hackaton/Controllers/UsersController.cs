﻿using System;
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

namespace hackaton.Controllers
{
    public class UsersController : Controller
    {
        private readonly Context _context;
        private readonly UserCacheService _userCacheService;
        public UsersController(Context context, UserCacheService cache)
        {
            _context = context;
            _userCacheService = cache;
        }

        // private readonly Context context;
        public IActionResult AllowedRegister(string cpf)
        {
            Console.WriteLine(cpf);

            if (_userCacheService.GetUserByCPFAsync(cpf) == null)
            {
                return Json(true);
            }

            return Json("CPF já cadastrado");

        }

       // [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        // GET: Users
        public async Task<IActionResult> Index()
        {
              return _context.Users != null ? 
                          View(_context.Users.Where(user => user.Active == true).ToList()) :
                          Problem("Entity set 'Context.Users'  is null.");
        }

        // GET: Users/Details/5
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]

        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        public IActionResult Create()
        {
          
            return View();
        }

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
                var cpfExists = await _context.Users.AnyAsync(u => u.CPF == user.CPF);
                if (cpfExists)
                {
                    ModelState.AddModelError("CPF", "O CPF já está cadastrado.");
                    return View(user);
                }

                string password = user.Password;
                user.Password = BCryptHelper.HashPassword(password, BCryptHelper.GenerateSalt());
                _context.Add(user);
                await _context.SaveChangesAsync();
                _userCacheService.AddUserToCache(user);
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [ServiceFilter(typeof(RequireLoginAdminAttributeFactory))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Password,CPF,IsAdmin")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string password = user.Password;
                    user.Password = BCryptHelper.HashPassword(password, BCryptHelper.GenerateSalt());
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'Context.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Active = false;
                _context.Users.Update(user);
                _context.SaveChangesAsync();
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
