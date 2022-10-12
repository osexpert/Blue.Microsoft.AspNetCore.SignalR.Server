using System;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	public class DataProtectionProviderProtectedData : IProtectedData
	{
		private static readonly UTF8Encoding _encoding = new UTF8Encoding(false, true);

		private readonly IDataProtectionProvider _provider;

		private readonly IDataProtector _connectionTokenProtector;

		private readonly IDataProtector _groupsProtector;

		public DataProtectionProviderProtectedData(IDataProtectionProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}
			_provider = provider;
			_connectionTokenProtector = provider.CreateProtector("SignalR.ConnectionToken");
			_groupsProtector = provider.CreateProtector("SignalR.Groups.v1.1");
		}

		public string Protect(string data, string purpose)
		{
			IDataProtector dataProtector = GetDataProtector(purpose);
			byte[] bytes = _encoding.GetBytes(data);
			return Convert.ToBase64String(dataProtector.Protect(bytes));
		}

		public string Unprotect(string protectedValue, string purpose)
		{
			IDataProtector dataProtector = GetDataProtector(purpose);
			byte[] protectedData = Convert.FromBase64String(protectedValue);
			byte[] array = dataProtector.Unprotect(protectedData);
			return _encoding.GetString(array, 0, array.Length);
		}

		private IDataProtector GetDataProtector(string purpose)
		{
			if (!(purpose == "SignalR.ConnectionToken"))
			{
				if (purpose == "SignalR.Groups.v1.1")
				{
					return _groupsProtector;
				}
				return _provider.CreateProtector(purpose);
			}
			return _connectionTokenProtector;
		}
	}
}
