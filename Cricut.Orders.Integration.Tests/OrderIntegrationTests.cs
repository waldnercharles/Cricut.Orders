using AutoBogus;
using Cricut.Orders.Api.ViewModels;
using FluentAssertions;
using System.Net.Http.Json;

namespace Cricut.Orders.Integration.Tests
{
    [TestClass]
    public class OrderIntegrationTests
    {
        [DataTestMethod]
        [DataRow(3, 2, 1.5, false)]
        [DataRow(3, 2, 1.5, false)]
        [DataRow(1, 1, 25, false)]
        [DataRow(3, 4, 8, true)]
        [DataRow(1, 1, 30, true)]
        public async Task CreateNewOrder_Does_Apply_Discount(int lineItems, int quantityOfEach, double priceOfEach, bool shouldApplyDiscount)
        {
            var newOrderBelowDiscount = CreateOrderWithItems(lineItems, quantityOfEach, priceOfEach);
            var client = OrdersApiTestClientFactory.CreateTestClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "v1/orders");
            request.Content = JsonContent.Create(newOrderBelowDiscount);

            var response = await client.SendAsync(request);
            response.IsSuccessStatusCode.Should().BeTrue();
            var order = await response.Content.ReadFromJsonAsync<OrderViewModel>();

            order.Should().BeEquivalentTo(newOrderBelowDiscount);

            var expectedTotal = (lineItems * quantityOfEach * priceOfEach);
            var expectedTotalMinusDiscount = expectedTotal - (expectedTotal * .1);
            if (shouldApplyDiscount)
            {
                order!.Total.Should().Be(expectedTotalMinusDiscount);
            }
            else
            {
                order!.Total.Should().Be(expectedTotal);
            }
        }
        
                [TestMethod]
        public async Task CreateNewOrder_Bug()
        {
            // Arrange - Reproduce the exact scenario from the bug report
            var newOrder = new NewOrderViewModel
            {
                Customer = new CustomerViewModel
                {
                    Id = 1,
                    Name = "John Doe",
                    Address = "123 Street",
                    Email = "john@cool.com"
                },
                OrderItems = new[]
                {
                    new OrderItemViewModel
                    {
                        Product = new ProductViewModel
                        {
                            Id = 1,
                            Name = "Product 1",
                            Price = 13.50
                        },
                        Quantity = 1
                    },
                    new OrderItemViewModel
                    {
                        Product = new ProductViewModel
                        {
                            Id = 2,
                            Name = "Product 2",
                            Price = 11.50
                        },
                        Quantity = 1
                    }
                }
            };

            var client = OrdersApiTestClientFactory.CreateTestClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "v1/orders");
            request.Content = JsonContent.Create(newOrder);
            
            var response = await client.SendAsync(request);
            response.IsSuccessStatusCode.Should().BeTrue();
            
            var order = await response.Content.ReadFromJsonAsync<OrderViewModel>();
            order!.Total.Should().Be(22.50);
        }


        private NewOrderViewModel CreateOrderWithItems(int numberOfLineItems, int quantityOfEachItem, double priceOfEachItem)
        {
            var orderItems = new AutoFaker<OrderItemViewModel>()
                .RuleFor(x => x.Quantity, quantityOfEachItem)
                .RuleFor(x => x.Product, new AutoFaker<ProductViewModel>()
                    .RuleFor(x => x.Id, p => p.Random.Int(min: 1))
                    .RuleFor(x => x.Price, priceOfEachItem))
                .Generate(numberOfLineItems)
                .ToArray();

            return new AutoFaker<NewOrderViewModel>()
                .RuleFor(x => x.OrderItems, orderItems);
        }
    }
}
