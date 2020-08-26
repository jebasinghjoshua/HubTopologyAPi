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
    private const string virtualMachineQuery = @"Resources
      | project name, location, type, tenantId, resourceGroup, properties.extended.instanceView.powerState.code, properties.extended.instanceView.powerState.displayStatus,properties.hardwareProfile.vmSize, properties.storageProfile.dataDisks[0].diskSizeGB, properties.storageProfile.osDisk.diskSizeGB, sku, tags.Environment, tags.CustomerID, tags.Purpose, tags.Scheduled_Downtime, tags.Schedule
      | where type =~ 'Microsoft.Compute/virtualMachines' 
      and name == '{0}' and resourceGroup == '{1}'
      and tenantId =~ 'd84d2141-1d34-48e4-b50a-8d3021f84df4'
      | order by name desc | limit 1";

    private const string virtualNetworkQuery = @"Resources
      | project name, location, type, tenantId, resourceGroup, properties.addressSpace.addressPrefixes[0]
      | where type =~ 'Microsoft.Network/virtualNetworks' 
      and name == '{0}' and resourceGroup == '{1}'
      and tenantId =~ 'd84d2141-1d34-48e4-b50a-8d3021f84df4'
      | order by name desc | limit 1";

    private const string serviceBusQuery = @"Resources
      | project name, location, type, tenantId, resourceGroup, sku.name, sku.tier, properties.status, properties.serviceBusEndpoint
      | where type =~ 'Microsoft.ServiceBus/namespaces' 
      and name == '{0}' and resourceGroup == '{1}'
      and tenantId =~ 'd84d2141-1d34-48e4-b50a-8d3021f84df4'
      | order by name desc | limit 1";

    private const string searchServiceQuery = @"Resources
      | project name, location, type, tenantId, resourceGroup, sku.name, properties.status, properties.publicNetworkAccess
      | where type =~ 'Microsoft.Search/searchServices' 
      and name == '{0}' and resourceGroup == '{1}'
      and tenantId =~ 'd84d2141-1d34-48e4-b50a-8d3021f84df4'
      | order by name desc | limit 1";

    private const string _certificate = "42ef9695addc71f27253450c4987fb4ec49b08a9";
    private const string _clientId = "88323e7c-dd3c-48a5-aff9-532ebe52c4ca";
    private const string _tenantId = "d84d2141-1d34-48e4-b50a-8d3021f84df4";

    public async Task<ResourceDetailModel> GetStatus(ResourceDetailModel resourceDetailModel, CancellationToken cancellationToken)
    {
      //APP
      if (resourceDetailModel.AppServerVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.AppServerVirtualMachine.Name))
      {
        var vmQuery = string.Format(virtualMachineQuery, resourceDetailModel.AppServerVirtualMachine.Name, resourceDetailModel.AppServerVirtualMachine.ResourceGroup);
        var result = await GetResourceStatus(vmQuery, cancellationToken);
        if (result != null) resourceDetailModel.AppServerVirtualMachine.UpdateDetail(result);
      }

      //DB
      if (resourceDetailModel.DbServerVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.DbServerVirtualMachine.Name))
      {
        var vmQuery = string.Format(virtualMachineQuery, resourceDetailModel.DbServerVirtualMachine.Name, resourceDetailModel.DbServerVirtualMachine.ResourceGroup);
        var result = await GetResourceStatus(vmQuery, cancellationToken);
        if (result != null) resourceDetailModel.DbServerVirtualMachine.UpdateDetail(result);
      }

      //Oct
      if (resourceDetailModel.OctopusVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.OctopusVirtualMachine.Name))
      {
        var vmQuery = string.Format(virtualMachineQuery, resourceDetailModel.OctopusVirtualMachine.Name, resourceDetailModel.OctopusVirtualMachine.ResourceGroup);
        var result = await GetResourceStatus(vmQuery, cancellationToken);
        if (result != null) resourceDetailModel.OctopusVirtualMachine.UpdateDetail(result);
      }

      //Insight
      if (resourceDetailModel.InsightsVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.InsightsVirtualMachine.Name))
      {
        var vmQuery = string.Format(virtualMachineQuery, resourceDetailModel.InsightsVirtualMachine.Name, resourceDetailModel.InsightsVirtualMachine.ResourceGroup);
        var result = await GetResourceStatus(vmQuery, cancellationToken);
        if (result != null) resourceDetailModel.InsightsVirtualMachine.UpdateDetail(result);
      }

      //Producer
      if (resourceDetailModel.ProducerVirtualMachine != null && !string.IsNullOrEmpty(resourceDetailModel.ProducerVirtualMachine.Name))
      {
        var vmQuery = string.Format(virtualMachineQuery, resourceDetailModel.ProducerVirtualMachine.Name, resourceDetailModel.ProducerVirtualMachine.ResourceGroup);
        var result = await GetResourceStatus(vmQuery, cancellationToken);
        if (result != null) resourceDetailModel.ProducerVirtualMachine.UpdateDetail(result);
      }

      //ServiceBus
      if (resourceDetailModel.ServiceBus != null && !string.IsNullOrEmpty(resourceDetailModel.ServiceBus.Name))
      {
        var vmQuery = string.Format(serviceBusQuery, resourceDetailModel.ServiceBus.Name, resourceDetailModel.ServiceBus.ResourceGroup);
        var result = await GetResourceStatus(vmQuery, cancellationToken);
        if (result != null) resourceDetailModel.ServiceBus.UpdateDetail(result);
      }

      //Search Service
      if (resourceDetailModel.SeachService != null && !string.IsNullOrEmpty(resourceDetailModel.SeachService.Name))
      {
        var vmQuery = string.Format(searchServiceQuery, resourceDetailModel.SeachService.Name, resourceDetailModel.SeachService.ResourceGroup);
        var result = await GetResourceStatus(vmQuery, cancellationToken);
        if (result != null) resourceDetailModel.SeachService.UpdateDetail(result);
      }

      //virtual network
      if (resourceDetailModel.VirtualNetwork != null && !string.IsNullOrEmpty(resourceDetailModel.VirtualNetwork.Name))
      {
        var vmQuery = string.Format(virtualNetworkQuery, resourceDetailModel.VirtualNetwork.Name, resourceDetailModel.VirtualNetwork.ResourceGroup);
        var result = await GetResourceStatus(vmQuery, cancellationToken);
        if (result != null) resourceDetailModel.VirtualNetwork.UpdateDetail(result);
      }

      return resourceDetailModel;
    }

    private async Task<ResourceDetailTable> GetResourceStatus(string vmQuery, CancellationToken cancellationToken)
    {
      var payload = System.Text.Json.JsonSerializer.Serialize(new
      {
        query = vmQuery,

      });
      var restCall = new Uri($"https://management.azure.com/providers/Microsoft.ResourceGraph/resources?api-version=2020-04-01-preview");

      var subscription = new Subscription
      {
        CertificateThumbPrint = _certificate,
        ClientId = _clientId,
        TenantId = _tenantId
      };

      var response = await Send(restCall, HttpMethod.Post, subscription, payload, cancellationToken);
      return FormatResponseObject(await response.Content.ReadAsStringAsync());
    }

    private async Task<HttpResponseMessage> Send(Uri url, HttpMethod method, Subscription subscription, string payload, CancellationToken cancellationToken)
    {
      var httpRequestMessage = new HttpRequestMessage
      {
        Method = method,
        RequestUri = url,
        Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), await AuthorizationHeaderValue(subscription) },
                    { "Prefer", "response-v1=true" }
                },
        Content = new StringContent(payload, Encoding.UTF8, "application/json")
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

    private ResourceDetailTable FormatResponseObject(string jsonData)
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

      return (System.Text.Json.JsonSerializer.Deserialize<ResourceDetailTable[]>(dtResult)).FirstOrDefault();

    }
  }
}
