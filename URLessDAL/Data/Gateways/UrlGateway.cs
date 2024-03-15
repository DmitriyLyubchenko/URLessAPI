using Microsoft.EntityFrameworkCore;
using URLessDAL.Entities;

namespace URLessDAL.Data.Gateways
{
    public class UrlGateway : IUrlGateway
    {
        private readonly DataContext _dataContext;

        public UrlGateway(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async ValueTask<Url> Create(string id, string original) 
        {
            var added = await _dataContext.AddAsync(new Url { Id = id, Original = original });
            await _dataContext.SaveChangesAsync();

            return added.Entity;
        }

        public ValueTask<Url?> Get(string id) 
        {
            return _dataContext.Urls.FindAsync(id);
        }

        public Task<bool> IsPresent(string id)
        {
            return _dataContext.Urls.AnyAsync(x => x.Id == id);
        }
    }
}
