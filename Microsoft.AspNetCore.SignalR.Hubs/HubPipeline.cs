using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal class HubPipeline : IHubPipelineInvoker
	{
		private class ComposedPipeline
		{
			public Func<IHubIncomingInvokerContext, Task<object>> Invoke;

			public Func<IHub, Task> Connect;

			public Func<IHub, Task> Reconnect;

			public Func<IHub, bool, Task> Disconnect;

			public Func<HubDescriptor, HttpRequest, bool> AuthorizeConnect;

			public Func<HubDescriptor, HttpRequest, IList<string>, IList<string>> RejoiningGroups;

			public Func<IHubOutgoingInvokerContext, Task> Send;

			public ComposedPipeline(IEnumerable<IHubPipelineModule> modules)
			{
				Invoke = Compose(modules, (IHubPipelineModule m, Func<IHubIncomingInvokerContext, Task<object>> f) => m.BuildIncoming(f))(HubDispatcher.Incoming);
				Connect = Compose(modules, (IHubPipelineModule m, Func<IHub, Task> f) => m.BuildConnect(f))(HubDispatcher.Connect);
				Reconnect = Compose(modules, (IHubPipelineModule m, Func<IHub, Task> f) => m.BuildReconnect(f))(HubDispatcher.Reconnect);
				Disconnect = Compose(modules, (IHubPipelineModule m, Func<IHub, bool, Task> f) => m.BuildDisconnect(f))(HubDispatcher.Disconnect);
				AuthorizeConnect = Compose(modules, (IHubPipelineModule m, Func<HubDescriptor, HttpRequest, bool> f) => m.BuildAuthorizeConnect(f))((HubDescriptor h, HttpRequest r) => true);
				RejoiningGroups = Compose(modules, (IHubPipelineModule m, Func<HubDescriptor, HttpRequest, IList<string>, IList<string>> f) => m.BuildRejoiningGroups(f))((HubDescriptor h, HttpRequest r, IList<string> g) => g);
				Send = Compose(modules, (IHubPipelineModule m, Func<IHubOutgoingInvokerContext, Task> f) => m.BuildOutgoing(f))(HubDispatcher.Outgoing);
			}

			private static Func<T, T> Compose<T>(IEnumerable<IHubPipelineModule> modules, Func<IHubPipelineModule, T, T> method)
			{
				return modules.Aggregate<IHubPipelineModule, Func<T, T>>((T x) => x, (Func<T, T> a, IHubPipelineModule b) => (T x) => method(b, a(x)));
			}
		}

		private readonly Lazy<ComposedPipeline> _pipeline;

		private ComposedPipeline Pipeline => _pipeline.Value;

		public HubPipeline(IOptions<SignalROptions> options)
		{
			_pipeline = new Lazy<ComposedPipeline>(() => new ComposedPipeline(options.get_Value().Hubs.PipelineModules));
		}

		public Task<object> Invoke(IHubIncomingInvokerContext context)
		{
			return Pipeline.Invoke(context);
		}

		public Task Connect(IHub hub)
		{
			return Pipeline.Connect(hub);
		}

		public Task Reconnect(IHub hub)
		{
			return Pipeline.Reconnect(hub);
		}

		public Task Disconnect(IHub hub, bool stopCalled)
		{
			return Pipeline.Disconnect(hub, stopCalled);
		}

		public bool AuthorizeConnect(HubDescriptor hubDescriptor, HttpRequest request)
		{
			return Pipeline.AuthorizeConnect(hubDescriptor, request);
		}

		public IList<string> RejoiningGroups(HubDescriptor hubDescriptor, HttpRequest request, IList<string> groups)
		{
			return Pipeline.RejoiningGroups(hubDescriptor, request, groups);
		}

		public Task Send(IHubOutgoingInvokerContext context)
		{
			return Pipeline.Send(context);
		}
	}
}
