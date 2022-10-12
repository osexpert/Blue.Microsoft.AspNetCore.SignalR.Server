namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public interface IHubActivator
	{
		IHub Create(HubDescriptor descriptor);
	}
}
