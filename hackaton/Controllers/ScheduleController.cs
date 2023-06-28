using frontend_hackaton.Models;
using frontend_hackaton.Models.Desserializers;
using hackaton.Models;
using hackaton.Models.Caches;
using hackaton.Models.Injectors;
using Microsoft.AspNetCore.Mvc;


namespace hackaton.Controllers
{
    public class ScheduleController : Controller
    {

        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        public async Task<ActionResult> Index()
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            List<Schedule> agendamentos = await ApiRequest.getSchedules(userId);//_ctx.Schedules.Where(sch => sch.UserId == userId).ToList();

            return View("~/Views/Schedule/Index.cshtml", agendamentos);
        }

        public async Task<IActionResult> LoadPartialListAgendamentos()
        {
            List<Schedule> agendamentos;

            string cpf = HttpContext.Session.GetString("CPF");
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var users = await ApiRequest.getUsers(cpf);
            if(users == null)
            {
                return NotFound();
            }
            User user = users.FirstOrDefault();

            if (user != null && user.Id == userId && user.IsAdmin)
            {
                agendamentos = await ApiRequest.getSchedules();
            }
            else
            {
                agendamentos = await ApiRequest.getSchedules(userId);
            }

            return PartialView("~/Views/Modules/partial_list_agendamentos", agendamentos);
        }

        public IActionResult LoadPartialCardAgendar()
        {
            return PartialView("~/Views/Modules/partial_card_agendar");
        }

        // POST: AgendamentoController/Create
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule agendamento)
        {
            string cpf = HttpContext.Session.GetString("CPF");
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var users = await ApiRequest.getUsers(cpf);
            if (users == null)
            {
                return NotFound();
            }
            User user = users.FirstOrDefault();
            agendamento.UserId = user.Id;

            ApiResponse<Schedule> response = await ApiRequest.createSchedule(agendamento);
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
            }

                return RedirectToAction("Index");
        }

        // GET: AgendamentoController/Edit/5
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AgendamentoController/Edit/5
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
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

        // POST: AgendamentoController/Delete/5
        [ServiceFilter(typeof(RequireLoginAttributeFactory))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
           Schedule schedule = await ApiRequest.deleteSchedule(id);

            if(schedule == null)
            {
                return Unauthorized();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}