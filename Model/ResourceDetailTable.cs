namespace HubTopology_API.Model
{
  public class ResourceDetailTable
  {
    public string name { get; set; }
    public string location { get; set; }
    public string type { get; set; }
    public string tenantId { get; set; }
    public string resourceGroup { get; set; }
    public string properties_extended_instanceView_powerState_code { get; set; }
    public string properties_extended_instanceView_powerState_displayStatus { get; set; }
    public string properties_hardwareProfile_vmSize { get; set; }
    public string properties_storageProfile_dataDisks_0_diskSizeGB { get; set; }
    public string properties_storageProfile_osDisk_diskSizeGB { get; set; }
    public string sku_name { get; set; }
    public string sku_tier { get; set; }
    public string tags_Environment { get; set; }
    public string tags_CustomerID { get; set; }
    public string tags_Purpose { get; set; }
    public string tags_Scheduled_Downtime { get; set; }
    public string tags_Schedule { get; set; }
    public string properties_status { get; set; }
    public string properties_serviceBusEndpoint { get; set; }
    public string properties_publicNetworkAccess { get; set; }

    //for virtualnetwork
    public string properties_addressSpace_addressPrefixes_0 { get; set; }
  }
}
