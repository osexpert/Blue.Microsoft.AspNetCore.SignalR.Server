using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal class HubInvocationProgress
	{
		private static readonly ConcurrentDictionary<Type, Func<Func<object, Task>, HubInvocationProgress>> _progressCreateCache = new ConcurrentDictionary<Type, Func<Func<object, Task>, HubInvocationProgress>>();

		private volatile bool _complete;

		private readonly object _statusLocker = new object();

		private readonly Func<object, Task> _sendProgressFunc;

		private ILogger Logger
		{
			get;
			set;
		}

		protected HubInvocationProgress(Func<object, Task> sendProgressFunc)
		{
			_sendProgressFunc = sendProgressFunc;
		}

		public static HubInvocationProgress Create(Type progressGenericType, Func<object, Task> sendProgressFunc, ILogger logger)
		{
			if (!_progressCreateCache.TryGetValue(progressGenericType, out Func<Func<object, Task>, HubInvocationProgress> value))
			{
				value = (Func<Func<object, Task>, HubInvocationProgress>)RuntimeReflectionExtensions.GetRuntimeMethod(typeof(HubInvocationProgress), "Create", new Type[1]
				{
					typeof(Func<object, Task>)
				}).MakeGenericMethod(progressGenericType).CreateDelegate(typeof(Func<Func<object, Task>, HubInvocationProgress>));
				_progressCreateCache[progressGenericType] = value;
			}
			HubInvocationProgress hubInvocationProgress = value(sendProgressFunc);
			hubInvocationProgress.Logger = logger;
			return hubInvocationProgress;
		}

		public static HubInvocationProgress<T> Create<T>(Func<object, Task> sendProgressFunc)
		{
			return new HubInvocationProgress<T>(sendProgressFunc);
		}

		public void SetComplete()
		{
			lock (_statusLocker)
			{
				_complete = true;
			}
		}

		protected void DoReport(object value)
		{
			lock (_statusLocker)
			{
				if (_complete)
				{
					throw new InvalidOperationException(Resources.Error_HubProgressOnlyReportableBeforeMethodReturns);
				}
				_sendProgressFunc(value).Catch(Logger);
			}
		}
	}
	internal class HubInvocationProgress<T> : HubInvocationProgress, IProgress<T>
	{
		public HubInvocationProgress(Func<object, Task> sendProgressFunc)
			: base(sendProgressFunc)
		{
		}

		public void Report(T value)
		{
			DoReport(value);
		}
	}
}
