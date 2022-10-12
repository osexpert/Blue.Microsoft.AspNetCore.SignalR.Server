namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public abstract class Descriptor
	{
		public virtual string Name
		{
			get;
			set;
		}

		public virtual bool NameSpecified
		{
			get;
			set;
		}
	}
}
