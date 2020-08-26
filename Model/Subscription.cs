namespace HubTopology_API.Model
{
  public class Subscription
  {
    public string CertificateThumbPrint { get; internal set; }
    public string ClientId { get; internal set; }
    public string TenantId { get; internal set; }
    public string AzureId { get; internal set; }
  }
}
