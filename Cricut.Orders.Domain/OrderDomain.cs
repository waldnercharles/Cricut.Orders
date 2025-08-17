using Cricut.Orders.Domain.Models;

namespace Cricut.Orders.Domain
{
    public interface IOrderDomain
    {
        Task<Order> CreateNewOrderAsync(Order order);
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
    }

    public class OrderDomain : IOrderDomain
    {
        private readonly IOrderStore _orderStore;

        public OrderDomain(IOrderStore orderStore)
        {
            _orderStore = orderStore;
        }

        public async Task<Order> CreateNewOrderAsync(Order order)
        {
            var updatedOrder = await _orderStore.SaveOrderAsync(order);
            return updatedOrder;
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _orderStore.GetAllOrdersForCustomerAsync(customerId);
        }
    }
}
