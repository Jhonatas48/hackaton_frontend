using Microsoft.AspNetCore.Mvc;
using hackaton.Models;

namespace hackaton.Controllers
{
    public class QrCodeController : Controller
    {
     
        public async Task<IActionResult> GenerateQrCode(User user) {

           byte[] imageBytes= await ApiRequest.CreateQrCode(user);

            if(imageBytes == null || imageBytes.Length == 0)
            {
                return Problem("Error in generate the QrCode");
            }
            //retorna a imagem do QR Code
            return File(imageBytes, "image/png");

        }

      
    }
}
