using Microsoft.AspNetCore.DataProtection;
using System;
using System.Text;

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
			byte[] array = Convert.FromBase64String(protectedValue);
			byte[] array2 = dataProtector.Unprotect(array);
			return _encoding.GetString(array2, 0, array2.Length);
		}

		private IDataProtector GetDataProtector(string purpose)
		{
			if (purpose == "SignalR.ConnectionToken")
			{
				return _connectionTokenProtector;
			}
			if (purpose == "SignalR.Groups.v1.1")
			{
				return _groupsProtector;
			}
			return _provider.CreateProtector(purpose);
		}
	}
}
