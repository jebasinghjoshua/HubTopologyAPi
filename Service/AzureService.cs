using HubTopology_API.Model;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HubTopology_API.Service
{
    public class AzureService
    {
        // {0} - inserted with all OR queries
        private const string _query = @"Resources
            | project name, location, type, tenantId, resourceGroup, properties.extended.instanceView.powerState.code, properties.extended.instanceView.powerState.displayStatus,properties.hardwareProfile.vmSize, properties.storageProfile.dataDisks[0].diskSizeGB, properties.storageProfile.osDisk.diskSizeGB, tags.Environment, tags.CustomerID, tags.Purpose, tags.Scheduled_Downtime, tags.Schedule, properties.addressSpace.addressPrefixes[0], sku.name, sku.tier, properties.status, properties.serviceBusEndpoint, properties.publicNetworkAccess
            | where 
            {0}
            and tenantId =~ 'd84d2141-1d34-48e4-b50a-8d3021f84df4'
            | order by name desc";

        private const string _queryVm = @"(type =~ 'Microsoft.Compute/virtualMachines' and name == '{0}' and resourceGroup == '{1}')";
        private const string _queryNetwork = @"(type =~ 'Microsoft.Network/virtualNetworks' and name == '{0}' and resourceGroup == '{1}')";
        private const string _queryServiceBus = @"(type =~ 'Microsoft.ServiceBus/namespaces' and name == '{0}' and resourceGroup == '{1}')";
        private const string _querySearchService = @"(type =~ 'Microsoft.Search/searchServices' and name == '{0}' and resourceGroup == '{1}')";
        private const string _or = " or ";
        private const string _certificate = "42ef9695addc71f27253450c4987fb4ec49b08a9";
        private const string _clientId = "88323e7c-dd3c-48a5-aff9-532ebe52c4ca";
        private const string _tenantId = "d84d2141-1d34-48e4-b50a-8d3021f84df4";
        private const string _azureId = "BED2C651-39E9-4E31-A9CC-5621849F8303";
        private readonly Subscription _subscription;

        public AzureService()
        {
            _subscription = new Subscription
            {
                CertificateThumbPrint = _certificate,
                ClientId = _clientId,
                TenantId = _tenantId,
                AzureId = _azureId
            };
        }

        public async Task<ResourceDetailModel> GetStatus(ResourceDetailModel resourceDetailModel, CancellationToken cancellationToken)
        {
            var insertOr = false;
            var _whereQueries = string.Empty;
            //APP
            if (resourceDetailModel.AppServerVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.AppServerVirtualMachine.Name))
            {
                if (insertOr) _whereQueries += _or;
                _whereQueries += string.Format(_queryVm, resourceDetailModel.AppServerVirtualMachine.Name, resourceDetailModel.AppServerVirtualMachine.ResourceGroup);
                insertOr = true;
            }

            //DB
            if (resourceDetailModel.DbServerVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.DbServerVirtualMachine.Name))
            {
                if (insertOr) _whereQueries += _or;
                _whereQueries += string.Format(_queryVm, resourceDetailModel.DbServerVirtualMachine.Name, resourceDetailModel.DbServerVirtualMachine.ResourceGroup);
                insertOr = true;
            }

            //Oct
            if (resourceDetailModel.OctopusVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.OctopusVirtualMachine.Name))
            {
                if (insertOr) _whereQueries += _or;
                _whereQueries += string.Format(_queryVm, resourceDetailModel.OctopusVirtualMachine.Name, resourceDetailModel.OctopusVirtualMachine.ResourceGroup);
                insertOr = true;
            }

            //Insight
            if (resourceDetailModel.InsightsVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.InsightsVirtualMachine.Name))
            {
                if (insertOr) _whereQueries += _or;
                _whereQueries += string.Format(_queryVm, resourceDetailModel.InsightsVirtualMachine.Name, resourceDetailModel.InsightsVirtualMachine.ResourceGroup);
                insertOr = true;
            }

            //Producer
            if (resourceDetailModel.ProducerVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.ProducerVirtualMachine.Name))
            {
                if (insertOr) _whereQueries += _or;
                _whereQueries += string.Format(_queryVm, resourceDetailModel.ProducerVirtualMachine.Name, resourceDetailModel.ProducerVirtualMachine.ResourceGroup);
                insertOr = true;
            }

            //ServiceBus
            if (resourceDetailModel.ServiceBus != null && !string.IsNullOrEmpty(resourceDetailModel.ServiceBus.Name))
            {
                if (insertOr) _whereQueries += _or;
                _whereQueries += string.Format(_queryServiceBus, resourceDetailModel.ServiceBus.Name, resourceDetailModel.ServiceBus.ResourceGroup);
                insertOr = true;
            }

            //Search Service
            if (resourceDetailModel.SeachService != null && !string.IsNullOrEmpty(resourceDetailModel.SeachService.Name))
            {
                if (insertOr) _whereQueries += _or;
                _whereQueries += string.Format(_querySearchService, resourceDetailModel.SeachService.Name, resourceDetailModel.SeachService.ResourceGroup);
                insertOr = true;
            }

            //virtual network
            if (resourceDetailModel.VirtualNetwork != null && !string.IsNullOrEmpty(resourceDetailModel.VirtualNetwork.Name))
            {
                if (insertOr) _whereQueries += _or;
                _whereQueries += string.Format(_queryNetwork, resourceDetailModel.VirtualNetwork.Name, resourceDetailModel.VirtualNetwork.ResourceGroup);
                insertOr = true;
            }

            var result = await GetResourceStatus(string.Format(_query, _whereQueries), cancellationToken);
            if (resourceDetailModel.AppServerVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.AppServerVirtualMachine.Name) && result.Any(x => x.name == resourceDetailModel.AppServerVirtualMachine.Name))
            {
                resourceDetailModel.AppServerVirtualMachine.UpdateDetail(result.Where(x => x.name == resourceDetailModel.AppServerVirtualMachine.Name).FirstOrDefault());
            }
            if (resourceDetailModel.DbServerVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.DbServerVirtualMachine.Name) && result.Any(x => x.name == resourceDetailModel.DbServerVirtualMachine.Name))
            {
                resourceDetailModel.DbServerVirtualMachine.UpdateDetail(result.Where(x => x.name == resourceDetailModel.DbServerVirtualMachine.Name).FirstOrDefault());
            }
            if (resourceDetailModel.OctopusVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.OctopusVirtualMachine.Name) && result.Any(x => x.name == resourceDetailModel.OctopusVirtualMachine.Name))
            {
                resourceDetailModel.OctopusVirtualMachine.UpdateDetail(result.Where(x => x.name == resourceDetailModel.OctopusVirtualMachine.Name).FirstOrDefault());
            }
            if (resourceDetailModel.InsightsVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.InsightsVirtualMachine.Name) && result.Any(x => x.name == resourceDetailModel.InsightsVirtualMachine.Name))
            {
                resourceDetailModel.InsightsVirtualMachine.UpdateDetail(result.Where(x => x.name == resourceDetailModel.InsightsVirtualMachine.Name).FirstOrDefault());
            }
            if (resourceDetailModel.ProducerVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.ProducerVirtualMachine.Name) && result.Any(x => x.name == resourceDetailModel.ProducerVirtualMachine.Name))
            {
                resourceDetailModel.ProducerVirtualMachine.UpdateDetail(result.Where(x => x.name == resourceDetailModel.ProducerVirtualMachine.Name).FirstOrDefault());
            }
            if (resourceDetailModel.VirtualNetwork != null && !string.IsNullOrEmpty(resourceDetailModel.VirtualNetwork.Name) && result.Any(x => x.name == resourceDetailModel.VirtualNetwork.Name))
            {
                resourceDetailModel.VirtualNetwork.UpdateDetail(result.Where(x => x.name == resourceDetailModel.VirtualNetwork.Name).FirstOrDefault());
            }
            if (resourceDetailModel.ServiceBus != null && !string.IsNullOrEmpty(resourceDetailModel.ServiceBus.Name) && result.Any(x => x.name == resourceDetailModel.ServiceBus.Name))
            {
                resourceDetailModel.ServiceBus.UpdateDetail(result.Where(x => x.name == resourceDetailModel.ServiceBus.Name).FirstOrDefault());
            }
            if (resourceDetailModel.SeachService != null && !string.IsNullOrEmpty(resourceDetailModel.SeachService.Name) && result.Any(x => x.name == resourceDetailModel.SeachService.Name))
            {
                resourceDetailModel.SeachService.UpdateDetail(result.Where(x => x.name == resourceDetailModel.SeachService.Name).FirstOrDefault());
            }

            //Resurce Update
            if(resourceDetailModel.AppServerVirtualMachine != null && resourceDetailModel.DbServerVirtualMachine != null) {
                resourceDetailModel.Claims = resourceDetailModel.Billing = resourceDetailModel.Policy = resourceDetailModel.Party = new Product { State = getState(resourceDetailModel.AppServerVirtualMachine.State, resourceDetailModel.DbServerVirtualMachine.State) };
            }
            if (resourceDetailModel.InsightsVirtualMachine != null && resourceDetailModel.InsightsVirtualMachine.ResourceGroup !="" && resourceDetailModel.AppServerVirtualMachine != null)
            {
                resourceDetailModel.Insights = new Product { State = getState(resourceDetailModel.InsightsVirtualMachine.State, resourceDetailModel.AppServerVirtualMachine.State) };
            }
            if (resourceDetailModel.ProducerVirtualMachine != null && resourceDetailModel.ProducerVirtualMachine.ResourceGroup != "" && resourceDetailModel.DbServerVirtualMachine != null)
            {
                resourceDetailModel.Producer = new Product { State = getState(resourceDetailModel.ProducerVirtualMachine.State, resourceDetailModel.DbServerVirtualMachine.State) };
            }

            return resourceDetailModel;
        }

        public async Task<bool> StartAzureVm(string name, string resourceGroup)
        {
            var restCall = new Uri($"https://management.azure.com/subscriptions/{_subscription.AzureId}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/virtualMachines/{name}/start?api-version=2019-07-01");
            var response = await Send(restCall, HttpMethod.Post, _subscription);
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return true;
            }
            return false;
        }

        private async Task<ResourceDetailTable[]> GetResourceStatus(string vmQuery, CancellationToken cancellationToken)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                query = vmQuery,

            });
            var restCall = new Uri($"https://management.azure.com/providers/Microsoft.ResourceGraph/resources?api-version=2020-04-01-preview");

            var response = await Send(restCall, HttpMethod.Post, _subscription, payload);
            return FormatResponseObject(await response.Content.ReadAsStringAsync());
        }

        private async Task<HttpResponseMessage> Send(Uri url, HttpMethod method, Subscription subscription, string payload = null)
        {
            HttpRequestMessage httpRequestMessage = await GetRequest(url, method, subscription);
            if (payload != null) httpRequestMessage.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var httpClient = new HttpClient();

            return await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
        }

        private async Task<HttpRequestMessage> GetRequest(Uri url, HttpMethod method, Subscription subscription) =>
            new HttpRequestMessage
            {
                Method = method,
                RequestUri = url,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), await AuthorizationHeaderValue(subscription) },
                    { "Prefer", "response-v1=true" }
                },

            };

        private async Task<HttpResponseMessage> Send(Uri url, HttpMethod method, Subscription subscription, CancellationToken cancellationToken)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = url,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), await AuthorizationHeaderValue(subscription) },
                    { "Prefer", "response-v1=true" }
                }
            };

            using var httpClient = new HttpClient();

            return await httpClient.SendAsync(httpRequestMessage, cancellationToken);
        }

        private async Task<string> AuthorizationHeaderValue(Subscription subscription)
        {
            var cert = ReadCertificateFromStoreByThumbPrint(subscription.CertificateThumbPrint, DateTime.UtcNow);
            var credential = new ClientAssertionCertificate(subscription.ClientId, cert);
            var authContext = new AuthenticationContext($"https://login.windows.net/" + subscription.TenantId);
            var resource = @"https://management.azure.com/";
            var result = await authContext.AcquireTokenAsync(resource, credential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return $"Bearer {result.AccessToken}";
        }

        private static X509Certificate2 ReadCertificateFromStoreByThumbPrint(string certThumbPrint, DateTime dateTime)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certCollection = store.Certificates;
            var currentCerts = certCollection.Find(X509FindType.FindByTimeValid, dateTime, false);
            var signingCert = currentCerts.Find(X509FindType.FindByThumbprint, certThumbPrint, false);
            var cert = signingCert
                .OfType<X509Certificate2>()
                .OrderByDescending(c => c.NotBefore)
                .FirstOrDefault() ?? throw new ArgumentException($"Certificate not found. Thumbprint '{certThumbPrint}'.");
            store.Close();

            return cert;
        }

        private ResourceDetailTable[] FormatResponseObject(string jsonData)
        {
            var table = JsonConvert.DeserializeObject<ResultTable>(jsonData);

            if (table.count < 1)
            {
                return null;
            }

            var dt = new DataTable();
            var columns = table.data.columns.Select(x => new DataColumn(x.name)).ToArray();

            dt.Columns.AddRange(columns);

            foreach (var row in table.data.rows)
            {
                dt.Rows.Add(row.ToArray());
            }

            var dtResult = JsonConvert.SerializeObject(dt);

            return System.Text.Json.JsonSerializer.Deserialize<ResourceDetailTable[]>(dtResult);

        }
        private State getState(State appServer, State dbServer)
        {
            if (appServer == State.Running && dbServer == State.Running)
                return State.Running;
            else
                return State.Stopped;
        }
    }
}
