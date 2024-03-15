using URLess.Core.Interfaces;
using URLessCore.Interfaces;
using URLessCore.Models.ResponseModels;
using URLessDAL.Data.Gateways;

namespace URLessCore.Services
{
    public class UrlService : IUrlService
    {
        private readonly IUrlGateway _urlGateway;
        private readonly IIdGenerator _idGenerator;
        private readonly ICacheService _cache;

        private const int MaxRetry = 10;

        public UrlService(IUrlGateway urlGateway, IIdGenerator idGenerator, ICacheService cache)
        {
            _urlGateway = urlGateway;
            _idGenerator = idGenerator;
            _cache = cache;
        }

        public async ValueTask<UrlResponse?> GetUrl(string id)
        {
            var url = _cache.Get(id);

            if (url != null)
            {
                return new UrlResponse { Shortened = url.Id, Original = url.Original };
            }

            url = await _urlGateway.Get(id);

            if (url != null)
            {
                _cache.Set(url);
                return new UrlResponse { Shortened = url.Id, Original = url.Original };
            }

            return null;
        }

        public async ValueTask<UrlResponse> CreateUrl(string url) 
        {
            if (string.IsNullOrWhiteSpace(url)) 
            {
                throw new ArgumentNullException(nameof(url));
            }

            var id = await GenerateId(url);

            var added = await _urlGateway.Create(id, url);
            _cache.Set(added);

            return new UrlResponse { Shortened = id, Original = url };
        }

        private async ValueTask<string> GenerateId(string url) 
        {
            var id = _idGenerator.Generate(url);

            if (_cache.Get(id) == null && 
                !await _urlGateway.IsPresent(id))
            {
                return id;
            }

            for (var i = 0; ; i++)
            {
                if (i == MaxRetry)
                {
                    throw new InvalidOperationException($"Cannot generate shortened url for '{url}' after {MaxRetry} retries");
                }

                id = _idGenerator.Regenerate();

                if (_cache.Get(id) == null && 
                    !await _urlGateway.IsPresent(id))
                {
                    return id;
                }
            }
        }
    }
}
