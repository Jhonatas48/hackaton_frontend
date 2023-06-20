using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using hackaton.Models.Caches;

namespace hackaton.Models.Validations
{
    public class RequireLoginAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly UserCacheService _service;
        public RequireLoginAttribute(UserCacheService service)
        {
           _service = service;

        }

        //Chamado sempre que possuir o cabeçalho Authorization
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {

           //Obtem o cpf do usuario
           string cpf =  context.HttpContext.Session.GetString("CPF");
            //Obtem o id do usuario
           int? userId = context.HttpContext.Session.GetInt32("UserId");
           int? sessionId = context.HttpContext.Session.GetInt32("SessionId");

            if (string.IsNullOrEmpty(cpf) || userId == null || sessionId == null)// || !authorizationHeader.StartsWith("Bearer ") || authorizationHeader.StartsWith("Bearer undefined"))
           {
                
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
                //;

            }
            User user = await _service.GetUserByCPFAsync(cpf);
            if (user == null)
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;

            }
       
    }

  
   }
}
