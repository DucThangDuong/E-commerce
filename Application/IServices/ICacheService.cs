using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ICacheService
    {
        public Task<T?> GetAsync<T>(string key);
        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);
        public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    }
}
