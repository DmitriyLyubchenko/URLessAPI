using URLessCore.Models.ResponseModels;

namespace URLessCore.Interfaces
{
    public interface IUrlService
    {
        ValueTask<UrlResponse?> GetUrl(string id);

        ValueTask<UrlResponse> CreateUrl(string url);
    }
}
