namespace Microsoft.AspNetCore.SignalR
{
	public class Cookie
	{
		public string Name
		{
			get;
			private set;
		}

		public string Domain
		{
			get;
			private set;
		}

		public string Path
		{
			get;
			private set;
		}

		public string Value
		{
			get;
			private set;
		}

		public Cookie(string name, string value)
			: this(name, value, string.Empty, string.Empty)
		{
		}

		public Cookie(string name, string value, string domain, string path)
		{
			Name = name;
			Value = value;
			Domain = domain;
			Path = path;
		}
	}
}
