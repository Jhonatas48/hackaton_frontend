using hackaton.Models.Caches;
using hackaton.Models.DAO;
using hackaton.Models.Validations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace hackaton.Models.Injectors
{
    public class RequireLoginAdminAttributeFactory : IFilterFactory
    {
        private readonly UserCacheService _context;

        public RequireLoginAdminAttributeFactory(UserCacheService service)
        {
            _context = service;
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new RequireLoginAdminAttribute(_context);
        }

        public bool IsReusable => false;
    }

}
