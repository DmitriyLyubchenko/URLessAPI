using URLessDAL.Entities;

namespace URLessDAL.Data.Gateways
{
    public interface IUrlGateway
    {
        ValueTask<Url> Create(string id, string original);

        ValueTask<Url?> Get(string id);

        Task<bool> IsPresent(string id);
    }
}
