using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IOrderDetail : IRepository<OrderDetail>
    {
        void Update(OrderDetail obj);
        void Save();
    }
}
