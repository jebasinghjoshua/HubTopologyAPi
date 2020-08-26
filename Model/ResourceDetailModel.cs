using System;

namespace HubTopology_API.Model
{
  public class ResourceDetailModel
  {
    public VirtualMachineVm AppServerVirtualMachine { get; set; }
    public VirtualMachineVm DbServerVirtualMachine { get; set; }
    public VirtualMachineVm OctopusVirtualMachine { get; set; }
    public VirtualMachineVm InsightsVirtualMachine { get; set; }
    public VirtualMachineVm ProducerVirtualMachine { get; set; }
    public AzureSearchService SeachService { get; set; }
    public AzureServiceBus ServiceBus { get; set; }
    public VirtualNetwork VirtualNetwork { get; set; }
  }

  public class VirtualNetwork : Resource
  {

    public string AddressPrefix { get; set; }

    public void UpdateDetail(ResourceDetailTable resourceDetailTable)
    {
      Update(resourceDetailTable);
      AddressPrefix = resourceDetailTable.properties_addressSpace_addressPrefixes_0;
    }
  }

  public class AzureServiceBus : Resource
  {
    public void UpdateDetail(ResourceDetailTable resourceDetailTable)
    {
      Update(resourceDetailTable);
      SkuName = resourceDetailTable.sku_name;
      SkuTier = resourceDetailTable.sku_tier;
      Status = resourceDetailTable.properties_status;
      Endpoint = resourceDetailTable.properties_serviceBusEndpoint;
    }

    public string SkuName { get; set; }
    public string SkuTier { get; set; }
    public string Status { get; set; }
    public string Endpoint { get; set; }
  }

  public class AzureSearchService : Resource
  {

    public void UpdateDetail(ResourceDetailTable resourceDetailTable)
    {
      Update(resourceDetailTable);
      SkuName = resourceDetailTable.sku_name;
      Status = resourceDetailTable.properties_status;
      PublicNetworkAccess = resourceDetailTable.properties_publicNetworkAccess;
    }

    public string SkuName { get; set; }
    public string Status { get; set; }
    public string PublicNetworkAccess { get; set; }
  }

  public class VirtualMachineVm : Resource
  {

    public void UpdateDetail(ResourceDetailTable resourceDetailTable)
    {
      Update(resourceDetailTable);

      State = resourceDetailTable.properties_extended_instanceView_powerState_code.Contains("running") ? State.Running : State.Stopped;
      VmSize = resourceDetailTable.properties_hardwareProfile_vmSize;
      DiskSizeGB = resourceDetailTable.properties_storageProfile_dataDisks_0_diskSizeGB;
      CustomerID = resourceDetailTable.tags_CustomerID;
      Purpose = resourceDetailTable.tags_Purpose;
      Environment = resourceDetailTable.tags_Environment;
      OSSizeGB = resourceDetailTable.properties_storageProfile_osDisk_diskSizeGB;
      Scheduled_Downtime = resourceDetailTable.tags_Scheduled_Downtime;
      Schedule = resourceDetailTable.tags_Schedule;
    }

    public State State { get; set; }

    public string VmSize { get; set; }

    public string DiskSizeGB { get; set; }

    public string OSSizeGB { get; set; }

    public string Environment { get; set; }

    public string CustomerID { get; set; }

    public string Purpose { get; set; }

    public string Scheduled_Downtime { get; set; }

    public string Schedule { get; set; }

  }

  public class Resource
  {

    public string Name { get; set; }

    public string ResourceGroup { get; set; }

    public string Location { get; set; }

    public string Type { get; set; }

    public string TenantId { get; set; }

    protected void Update(ResourceDetailTable resourceDetailTable)
    {
      Location = resourceDetailTable.location;
      Type = resourceDetailTable.type;
      TenantId = resourceDetailTable.tenantId;
    }
  }

  public enum State
  {
    Stopped = 0,
    Running = 1
  }
}
