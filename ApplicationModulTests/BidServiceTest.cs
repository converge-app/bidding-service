using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Application;
using Application.Utility.Models;
using ApplicationModulTests.TestUtility;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace ApplicationIntegrationTests
{
    public class BidServiceTest : IClassFixture<WebApplicationFactory<StartupDevelopment>>
    {
        private readonly WebApplicationFactory<StartupDevelopment> _factory;

        public BidServiceTest(WebApplicationFactory<StartupDevelopment> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_Health_ping()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Act
            var response = await client.GetAsync("/api/health/ping");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task GEt_Health_ping()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/health/ping");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var actual = JsonConvert.DeserializeObject<MessageObj>(await response.Content.ReadAsStringAsync());
            Assert.Equal("pong!", actual.Message);
        }

        [Fact]
        public async Task Post_Projects()
        {
            Environment.SetEnvironmentVariable("USERS_SERVICE_HTTP", "users-service.api.converge-app.net");

            var client = _factory.CreateClient();

            var httpClient = new HttpClient();
            var authUser = await AuthUtility.GenerateAndAuthenticate(httpClient);
            AuthUtility.AddAuthorization(client, authUser.Token);
            var projectsClient = new HttpClient();
            AuthUtility.AddAuthorization(projectsClient, authUser.Token);
            

            var project = ProjectUtility.GenerateProject(authUser.Id);
            var pro = await ProjectUtility.CreateProject(projectsClient, project);

            Assert.NotNull(pro);
        }


        [Fact]
        public async Task Get_Biddings()
        {
            // Arrange
            Environment.SetEnvironmentVariable("USERS_SERVICE_HTTP", "users-service.api.converge-app.net");

            var client = _factory.CreateClient();

            var httpClient = new HttpClient();
            var authUser = await AuthUtility.GenerateAndAuthenticate(httpClient);

            AuthUtility.AddAuthorization(client, authUser.Token);

            // Act
            var response = await client.GetAsync("/api/Biddings");

            Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        }

    }
}