using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Infrastructure
{
	internal sealed class TaskQueue
	{
		private readonly object _lockObj = new object();

		private Task _lastQueuedTask;

		private volatile bool _drained;

		private readonly int? _maxSize;

		private long _size;

		public IPerformanceCounter QueueSizeCounter
		{
			get;
			set;
		}

		public bool IsDrained => _drained;

		public TaskQueue()
			: this(Microsoft.AspNetCore.SignalR.TaskAsyncHelper.Empty)
		{
		}

		public TaskQueue(Task initialTask)
		{
			_lastQueuedTask = initialTask;
		}

		public TaskQueue(Task initialTask, int maxSize)
		{
			_lastQueuedTask = initialTask;
			_maxSize = maxSize;
		}

		public Task Enqueue(Func<object, Task> taskFunc, object state)
		{
			lock (_lockObj)
			{
				if (_drained)
				{
					return _lastQueuedTask;
				}
				if (_maxSize.HasValue)
				{
					if (Interlocked.Increment(ref _size) > _maxSize)
					{
						Interlocked.Decrement(ref _size);
						return null;
					}
					QueueSizeCounter?.Increment();
				}
				return _lastQueuedTask = _lastQueuedTask.Then((Func<object, Task> n, object ns, Microsoft.AspNetCore.SignalR.Infrastructure.TaskQueue q) => q.InvokeNext(n, ns), taskFunc, state, this);
			}
		}

		private Task InvokeNext(Func<object, Task> next, object nextState)
		{
			return next(nextState).Finally(delegate(object s)
			{
				((Microsoft.AspNetCore.SignalR.Infrastructure.TaskQueue)s).Dequeue();
			}, this);
		}

		private void Dequeue()
		{
			if (_maxSize.HasValue)
			{
				Interlocked.Decrement(ref _size);
				QueueSizeCounter?.Decrement();
			}
		}

		public Task Enqueue(Func<Task> taskFunc)
		{
			return Enqueue((object state) => ((Func<Task>)state)(), taskFunc);
		}

		public Task Drain()
		{
			lock (_lockObj)
			{
				_drained = true;
				return _lastQueuedTask;
			}
		}
	}
}
