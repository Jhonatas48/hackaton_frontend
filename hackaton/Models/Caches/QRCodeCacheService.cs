using Microsoft.Extensions.Caching.Memory;

namespace hackaton.Models.Caches
{
    public class QRCodeCacheService
    {
        private readonly IMemoryCache _cache;
       
        public QRCodeCacheService(IMemoryCache cache)
        {
            _cache = cache;
         
        }

        public void AddQRCodeToCache(QrCode qrCode)
        {
            _cache.Set(qrCode.Content, qrCode);
        }


        public QrCode GetQRCodeByContentAsync(string content)
        {
            // Verifica se o usuário está no cache
            if (_cache.TryGetValue(content, out QrCode qrCode))
            {
                return qrCode;
            }

            return qrCode;
        }
    }
}

