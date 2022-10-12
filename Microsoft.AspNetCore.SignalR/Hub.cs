using Microsoft.AspNetCore.SignalR.Hubs;
using System;
using System.Threading.Tasks;

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
			return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
		}

		public virtual Task OnConnected()
		{
			return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
		}

		public virtual Task OnReconnected()
		{
			return Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty;
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
