using Microsoft.Extensions.Caching.Memory;
using URLess.Models;
using URLessDAL.Data.Gateways;
using URLessDAL.Entities;

namespace URLess.Services
{
    public class UrlService
    {
        private readonly IUrlGateway _urlGateway;

        private readonly IMemoryCache _memoryCache;

        private const int MaxRetry = 10;

        public UrlService(IUrlGateway urlGateway, IMemoryCache memoryCache)
        {
            _urlGateway = urlGateway;
            _memoryCache = memoryCache;
        }

        public async ValueTask<UrlResponse?> GetUrl(string id)
        {
            if (_memoryCache.TryGetValue<Url>(id, out var url))
            {
                return new UrlResponse { Shortened = url.Id, Original = url.Original };
            }

            url = await _urlGateway.Get(id);

            if (url == null)
            {
                return null;
            }

            return new UrlResponse { Shortened = url.Id, Original = url.Original };
        }

        public async ValueTask<UrlResponse> CreateUrl(string url) 
        {
            if (string.IsNullOrWhiteSpace(url)) 
            {
                throw new ArgumentNullException(nameof(url));
            }

            var id = await GenerateId(url);

            await _urlGateway.Create(id, url);
            _memoryCache.Set(id, url, TimeSpan.FromHours(1));

            return new UrlResponse { Shortened = id, Original = url };
        }

        private async ValueTask<string> GenerateId(string url) 
        {
            var generator = new IdGenerator(url);

            var id = generator.Generate();

            if (!_memoryCache.TryGetValue(id, out var _) && 
                !await _urlGateway.IsPresent(id))
            {
                return id;
            }

            for (var i = 0; true; i++)
            {
                if (i == MaxRetry)
                {
                    throw new InvalidOperationException($"Cannot generate shortened url for '{url}' after {MaxRetry} retries");
                }

                id = generator.Regenerate();

                if (!_memoryCache.TryGetValue(id, out var _) && 
                    !await _urlGateway.IsPresent(id))
                {
                    return id;
                }
            }
        }
    }
}
