using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SportsStore.Models
{
    public class EFOrderRepository : IOrderRepository
    {
        private StoreDbContext context;

        public EFOrderRepository(StoreDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<Order> Orders => context.Orders
            //Retrieve data from multiple tables
            .Include(o => o.Lines)
            .ThenInclude(l => l.Product);

        public void SaveOrder(Order order)
        {
            //Prevent EF from storing products, assosiated with the Order, because they
            //already exist in the database
            context.AttachRange(order.Lines.Select(l => l.Product));
            if( order.OrderID == 0)
            {
                context.Orders.Add(order);
            }
            context.SaveChanges();
        }
    }
}
