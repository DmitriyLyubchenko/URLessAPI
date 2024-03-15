using URLessDAL.Entities;

namespace URLess.Core.Interfaces
{
    public interface ICacheService
    {
        Url? Get(string id);

        void Set(Url url);
    }
}
