using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GameWeb.Models;
using System.Net.Http;
using System.Fabric;
using System.Fabric.Query;
using Newtonsoft.Json;
using System.Fabric.Description;

namespace GameWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly StatelessServiceContext _serviceContext;
        private readonly FabricClient _fabricClient;

        public HomeController(HttpClient httpClient, StatelessServiceContext serviceContext, FabricClient fabricClient)
        {
            _httpClient = httpClient;
            _serviceContext = serviceContext;
            _fabricClient = fabricClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = Request.Cookies["userid"];
            ViewBag.IsLoggedIn = userId != null;

            List<string> result = new List<string>();

            Uri serviceName = GameWeb.GetGameDataServiceName(_serviceContext);
            Uri proxyAddress = this.GetProxyAddress(serviceName);

            try
            {
                ServicePartitionList partitions = await _fabricClient.QueryManager.GetPartitionListAsync(serviceName);

                foreach (Partition partition in partitions)
                {
                    string proxyUrl =
                        $"{proxyAddress}/api/GamesData?PartitionKey={((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";

                    using (HttpResponseMessage response = await _httpClient.GetAsync(proxyUrl))
                    {
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            continue;
                        }

                        result.AddRange(JsonConvert.DeserializeObject<IEnumerable<string>>(await response.Content.ReadAsStringAsync()));
                    }
                }
            }
            catch
            {
            }

            return View(result.ToArray());
        }

        [HttpPost("")]
        public async Task<IActionResult> Index(string id, string mode)
        {
            if (mode == "creategame")
            {
                Uri serviceName = GameWeb.GetGameDataServiceName(_serviceContext);
                Uri proxyAddress = this.GetProxyAddress(serviceName);

                string proxyUrl =
                    $"{proxyAddress}/api/GamesData/{id}?PartitionKey={GetPartition(id)}&PartitionKind=Int64Range";

                using (HttpResponseMessage response = await _httpClient.PostAsync(proxyUrl, new StringContent("")))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        return BadRequest();
                    }

                    return Redirect($"Games/{id}");
                }
            }
            else if (mode == "signin")
            {
                Response.Cookies.Append("userid", id, new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTime.Now.AddDays(1), });

                return Redirect($"/");
            }
            else
            {
                return BadRequest();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
