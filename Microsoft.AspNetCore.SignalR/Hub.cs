using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace Microsoft.AspNetCore.SignalR
{
	public abstract class Hub : IHub, IDisposable
	{
		public IHubCallerConnectionContext<dynamic> Clients
		{
			get;
			set;
		}

		public HubCallerContext Context
		{
			get;
			set;
		}

		public IGroupManager Groups
		{
			get;
			set;
		}

		protected Hub()
		{
			Clients = new HubConnectionContext();
		}

		public virtual Task OnDisconnected(bool stopCalled)
		{
			return TaskAsyncHelper.Empty;
		}

		public virtual Task OnConnected()
		{
			return TaskAsyncHelper.Empty;
		}

		public virtual Task OnReconnected()
		{
			return TaskAsyncHelper.Empty;
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
	public abstract class Hub<T> : Hub where T : class
	{
		private IHubCallerConnectionContext<T> _testClients;

		public new IHubCallerConnectionContext<T> Clients
		{
			get
			{
				return _testClients ?? new TypedHubCallerConnectionContext<T>(base.Clients);
			}
			set
			{
				_testClients = value;
			}
		}
	}
}
