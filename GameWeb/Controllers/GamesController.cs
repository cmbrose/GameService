using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GameWeb.Controllers
{
    [Route("[controller]")]
    public class GamesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly StatelessServiceContext _serviceContext;
        private readonly FabricClient _fabricClient;

        public GamesController(HttpClient httpClient, StatelessServiceContext serviceContext, FabricClient fabricClient)
        {
            _httpClient = httpClient;
            _serviceContext = serviceContext;
            _fabricClient = fabricClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var userId = Request.Cookies["userid"];

            Uri serviceName = GameWeb.GetGameDataServiceName(_serviceContext);
            Uri proxyAddress = this.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/api/GamesData/AddPlayer/{id}?PartitionKey={GetPartition(id)}&PartitionKind=Int64Range";

            using (HttpResponseMessage response = await _httpClient.PutAsJsonAsync(proxyUrl, new { player = userId }))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ViewBag.Role = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    ViewBag.Role = "Spectator";
                }
            }

            proxyUrl =
                $"{proxyAddress}/api/GamesData/{id}?PartitionKey={GetPartition(id)}&PartitionKind=Int64Range";

            using (HttpResponseMessage response = await _httpClient.GetAsync(proxyUrl))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return NotFound();
                }

                string[] gameState = JsonConvert.DeserializeObject<string[]>(await response.Content.ReadAsStringAsync());
                return View(gameState);
            }
        }

        [HttpGet("{id}/GetState")]
        public async Task<IActionResult> GetState(string id)
        {
            Uri serviceName = GameWeb.GetGameDataServiceName(_serviceContext);
            Uri proxyAddress = this.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/api/GamesData/{id}?PartitionKey={GetPartition(id)}&PartitionKind=Int64Range";

            using (HttpResponseMessage response = await _httpClient.GetAsync(proxyUrl))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return NotFound();
                }

                return Ok(await response.Content.ReadAsStringAsync());
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, int idx)
        {
            var userId = Request.Cookies["userid"];

            Uri serviceName = GameWeb.GetGameDataServiceName(_serviceContext);
            Uri proxyAddress = this.GetProxyAddress(serviceName);

            string proxyUrl =
                $"{proxyAddress}/api/GamesData/{id}?PartitionKey={GetPartition(id)}&PartitionKind=Int64Range";

            using (HttpResponseMessage response = await _httpClient.PutAsJsonAsync(proxyUrl, new { player = userId, idx }))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return BadRequest(response.ReasonPhrase);
                }

                string[] result = JsonConvert.DeserializeObject<string[]>(await response.Content.ReadAsStringAsync());
                return Json(result);
            }
        }

        private Uri GetProxyAddress(Uri serviceName)
        {
            return new Uri($"http://localhost:19081{serviceName.AbsolutePath}");
        }

        private long GetPartition(string id)
        {
            return id.GetHashCode() % 1;
        }
    }
}