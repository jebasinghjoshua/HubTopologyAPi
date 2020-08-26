using HubTopology_API.Model;
using HubTopology_API.Service;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HubTopology_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/{id}")]
    public class ResourceDetailController : ControllerBase
    {
        private readonly AzureService _azureService;
        private readonly Client[] _clientData;
        public ResourceDetailController(AzureService azureService)
        {
            _azureService = azureService;
            //load all data
            string json = System.IO.File.ReadAllText("Data/Client.json");
            // get requested client data
            _clientData = JsonSerializer.Deserialize<Client[]>(json);
        }

        [HttpGet]
        public async Task<ResourceDetailModel> Get(int id)
        {
            //load client resource
            var resource = _clientData.SingleOrDefault(x => x.Id == id);
            return await _azureService.GetStatus(resource.ResourceDetail, CancellationToken.None);
        }

        [HttpPost]
        public async Task<bool> StartStopResource([FromQuery] string name, string resourceGroup, string action = null)
        {
            if (action == null) action = "start";
            return await _azureService.StartAzureVm(name, resourceGroup, action);

        }

        //[HttpGet(nameof(GetStatus))]
        //public async Task<ResourceDetailModel> GetStatus(ResourceDetailModel resourceDetail)
        //{
        //  return await _azureService.GetStatus(resourceDetail, CancellationToken.None);

        //}
    }
}
