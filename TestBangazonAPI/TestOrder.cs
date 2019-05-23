﻿using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace TestBangazonAPI
{

    public class TestOrder
    {
        public async Task<Order> createOrder(HttpClient client)
        {
            Order One = new Order
            {
                PaymentTypeId = 3,
                CustomerId = 3,
            };
            string orderAsJSON = JsonConvert.SerializeObject(One);


            HttpResponseMessage response = await client.PostAsync(
                "api/Orders",
                new StringContent(orderAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Order newOrder = JsonConvert.DeserializeObject<Order>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newOrder;

        }

        public async Task deleteOrder(Order One, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/Orders/{One.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task GetAllOrders()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/Orders");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list
                List<Order> orderList = JsonConvert.DeserializeObject<List<Order>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                Assert.True(orderList.Count > 0);
            }
        }

        [Fact]
        public async Task GetSingleOrder()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                Order newOrder = await createOrder(client);

                HttpResponseMessage response = await client.GetAsync($"api/Orders/{newOrder.Id}");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                Order order = JsonConvert.DeserializeObject<Order>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(3, newOrder.PaymentTypeId);
                Assert.Equal(3, newOrder.CustomerId);

                deleteOrder(newOrder, client);
            }
        }

        [Fact]
        public async Task includeProducts()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create new
                Order newOrder = await createOrder(client);

                // Try to get from the database
                HttpResponseMessage response = await client.GetAsync($"api/Orders/{newOrder.Id}/?include=products");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Order order = JsonConvert.DeserializeObject<Order>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(3, newOrder.PaymentTypeId);
                Assert.Equal(3, newOrder.CustomerId);
                Assert.NotNull(newOrder.Products);

                // Clean up
                deleteOrder(newOrder, client);
            }
        }

        [Fact]
        public async Task includeCustomer()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create new
                Order newOrder = await createOrder(client);

                // Try to get from the database
                HttpResponseMessage response = await client.GetAsync($"api/Orders/{newOrder.Id}/?include=customer");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Order order = JsonConvert.DeserializeObject<Order>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(3, newOrder.PaymentTypeId);
                Assert.Equal(3, newOrder.CustomerId);

                // Clean up
                deleteOrder(newOrder, client);
            }
        }

        [Fact]
        public async Task createThenDelete()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create new
                Order newOrder = await createOrder(client);

                // Make sure info checks out
                Assert.Equal(3, newOrder.PaymentTypeId);
                Assert.Equal(3, newOrder.CustomerId);

                // Clean up
                deleteOrder(newOrder, client);
            }
        }

        [Fact]
        public async Task modifiedOrder()
        {

            int newPaymentTypeId = 2;

            using (HttpClient client = new APIClientProvider().Client)
            {

                Order newOrder = await createOrder(client);

                newOrder.PaymentTypeId = newPaymentTypeId;

                string modifiedOrderAsJSON = JsonConvert.SerializeObject(newOrder);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/Orders/{newOrder.Id}",
                    new StringContent(modifiedOrderAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Try to GET the student we just edited
                HttpResponseMessage getOrder = await client.GetAsync($"api/Orders/{newOrder.Id}");
                getOrder.EnsureSuccessStatusCode();

                string getOrderBody = await getOrder.Content.ReadAsStringAsync();
                Order modifiedOrder = JsonConvert.DeserializeObject<Order>(getOrderBody);

                Assert.Equal(HttpStatusCode.OK, getOrder.StatusCode);

                Assert.Equal(newPaymentTypeId, modifiedOrder.PaymentTypeId);

                // Clean up
                deleteOrder(modifiedOrder, client);
            }
        }
    }
}