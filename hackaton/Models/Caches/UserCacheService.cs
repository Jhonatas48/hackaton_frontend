using Microsoft.Extensions.Caching.Memory;

namespace hackaton.Models.Caches
{

    /*
        Classe Responsável para armazenar os usuarios em cache,para melhor performance na aplicação
     */
    public class UserCacheService
    {
        private readonly IMemoryCache _cache;
       
        public UserCacheService(IMemoryCache cache)
        {
            _cache = cache;
           
        }

        public void AddUserToCache(User user)
        {
            _cache.Set(user.CPF, user);
        }


        public async Task<User> GetUserByCPFAsync(string cpf)
        {
            // Verifica se o usuário está no cache
            if (_cache.TryGetValue(cpf, out User user))
            {
                return user;
            }

            var users = await ApiRequest.getUsers(cpf);
            if (users == null)
                return null;
            // Se o usuário não está no cache, busca no banco de dados
            user = users.FirstOrDefault();

            // Adiciona o usuário ao cache por 5 minutos
            if (user != null)
            {
                _cache.Set(cpf, user, TimeSpan.FromMinutes(5));
            }

            return user;
        }
    }
}
