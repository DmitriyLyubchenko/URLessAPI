using Microsoft.Extensions.Caching.Memory;
using URLess.Core.Interfaces;
using URLessDAL.Entities;

namespace URLess.Core.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        private readonly TimeSpan _expiredAfter = TimeSpan.FromHours(1);

        public CacheService(IMemoryCache memoryCache) 
        {
            _memoryCache = memoryCache;
        }

        public Url? Get(string id) 
        {
            if (_memoryCache.TryGetValue<Url?>(id, out var url)) 
            {
                return url;
            }

            return null;
        }

        public void Set(Url url)
        {
            _memoryCache.Set(url.Id, url, _expiredAfter);
        }
    }
}
