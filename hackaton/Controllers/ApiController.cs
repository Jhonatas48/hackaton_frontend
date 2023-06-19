﻿using Microsoft.AspNetCore.Mvc;
using hackaton.Models.Security;
using hackaton.Models;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using hackaton.Models.DAO;
using hackaton.Models.WebSocket;
using Microsoft.AspNetCore.SignalR;

namespace hackaton.Controllers
{
    public class ApiController : Controller
    {

        private readonly HomeController _homeController;
        private readonly Context _context;
        private readonly IHubContext<RedirectClient> _redirectClient;
        public ApiController(HomeController homeController,Context context, IHubContext<RedirectClient> redirectClient)
       {
         _homeController = homeController;
         _context = context;
         _redirectClient = redirectClient;

        }
    // GET: ApiController/QRCode
    [BearerAuthorize]
 
        public async Task<ActionResult> QRCodeAsync([FromBody] QrCode qrCode)
        {
            if (qrCode == null || string.IsNullOrEmpty(qrCode.Content)) {
                return new BadRequestObjectResult(new { message = "QrCode is required" });
            }

            QrCode qrCodeRetrieve = _context.QrCodes.Where(qr => qr.Content.Equals(qrCode.Content)).FirstOrDefault();
            
            if(qrCodeRetrieve == null)
            {
                return NotFound();
            }
            qrCode = new QrCode
            {
                     QRCodeId = qrCodeRetrieve.QRCodeId,
                    Content = qrCodeRetrieve.Content,
                    User = qrCodeRetrieve.User,
                    UserId = qrCodeRetrieve.UserId,
                    Expired = qrCodeRetrieve.Expired,
                    TimeExpiration = qrCodeRetrieve.TimeExpiration

             };

            qrCodeRetrieve.Expired = true;
            //_context.Update(qrCodeRetrieve);
            _context.QrCodes.Remove(qrCodeRetrieve);
            _context.SaveChanges();

            if (qrCodeRetrieve.TimeExpiration <= DateTime.Now || qrCode.Expired)
            {
                var message = new
                {
                    Message = "QrCode is expired"

                };
                return new UnauthorizedObjectResult(message);
            }
            //Direciona o usuario para o /Client/Index
            await _redirectClient.Clients.Group("pc_user"+qrCode.UserId).SendAsync("redirect","/Client/Index");
            return Json(qrCodeRetrieve);
        }

        // GET: ApiController/Details/5
        public async Task<ActionResult> validadeUser([FromBody] User user)
        {
            if(user == null)
            {
                return new BadRequestObjectResult(new { message = "User is required" });
            }

            if (!ModelState.IsValid)
            {
                ModelState.Remove("Name");
                ModelState.Remove("Properties");

            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (await _homeController.validateLogin(user))
            {
                return Ok(user);
            }

            return new UnauthorizedObjectResult(new { message = "Invalid Credentials" });
        }

        // GET: ApiController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ApiController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ApiController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ApiController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ApiController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ApiController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
