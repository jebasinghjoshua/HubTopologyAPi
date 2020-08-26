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


        [Route("api/[controller]/{id}")]
        [HttpGet]
        public async Task<ResourceDetailModel> Get(int id)
        {
            //load client resource
            var resource = _clientData.SingleOrDefault(x => x.Id == id);
            return await _azureService.GetStatus(resource.ResourceDetail, CancellationToken.None);
        }


        [Route("api/[controller]")]
        [HttpPost]
        public async Task<bool> StartResource([FromQuery] string name, string resourceGroup) => await _azureService.StartAzureVm(name, resourceGroup);

    }
}
