using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SignalR
{
	internal static class TaskAsyncHelper
	{
		private static class TaskRunners<T, TResult>
		{
			internal static Task RunTask(Task<T> task, Action<T> successor)
			{
				TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
				task.ContinueWithPreservedCulture(delegate(Task<T> t)
				{
					if (t.IsFaulted)
					{
						tcs.SetUnwrappedException(t.Exception);
					}
					else if (t.IsCanceled)
					{
						tcs.SetCanceled();
					}
					else
					{
						try
						{
							successor(t.Result);
							tcs.SetResult(null);
						}
						catch (Exception e)
						{
							tcs.SetUnwrappedException(e);
						}
					}
				});
				return tcs.Task;
			}

			internal static Task<TResult> RunTask(Task task, Func<TResult> successor)
			{
				TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
				task.ContinueWithPreservedCulture(delegate(Task t)
				{
					if (t.IsFaulted)
					{
						tcs.SetUnwrappedException(t.Exception);
					}
					else if (t.IsCanceled)
					{
						tcs.SetCanceled();
					}
					else
					{
						try
						{
							tcs.SetResult(successor());
						}
						catch (Exception e)
						{
							tcs.SetUnwrappedException(e);
						}
					}
				});
				return tcs.Task;
			}

			internal static Task<TResult> RunTask(Task<T> task, Func<Task<T>, TResult> successor)
			{
				TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
				task.ContinueWithPreservedCulture(delegate(Task<T> t)
				{
					if (task.IsFaulted)
					{
						tcs.SetUnwrappedException(t.Exception);
					}
					else if (task.IsCanceled)
					{
						tcs.SetCanceled();
					}
					else
					{
						try
						{
							tcs.SetResult(successor(t));
						}
						catch (Exception e)
						{
							tcs.SetUnwrappedException(e);
						}
					}
				});
				return tcs.Task;
			}
		}

		private static class GenericDelegates<T, TResult, T1, T2, T3>
		{
			internal static Task ThenWithArgs(Task task, Action<T1> successor, T1 arg1)
			{
				return RunTask(task, delegate
				{
					successor(arg1);
				});
			}

			internal static Task ThenWithArgs(Task task, Action<T1, T2> successor, T1 arg1, T2 arg2)
			{
				return RunTask(task, delegate
				{
					successor(arg1, arg2);
				});
			}

			internal static Task<TResult> ThenWithArgs(Task task, Func<T1, TResult> successor, T1 arg1)
			{
				return TaskRunners<object, TResult>.RunTask(task, () => successor(arg1));
			}

			internal static Task<TResult> ThenWithArgs(Task task, Func<T1, T2, TResult> successor, T1 arg1, T2 arg2)
			{
				return TaskRunners<object, TResult>.RunTask(task, () => successor(arg1, arg2));
			}

			internal static Task<TResult> ThenWithArgs(Task<T> task, Func<T, T1, TResult> successor, T1 arg1)
			{
				return TaskRunners<T, TResult>.RunTask(task, (Task<T> t) => successor(t.Result, arg1));
			}

			internal static Task<Task> ThenWithArgs(Task task, Func<T1, Task> successor, T1 arg1)
			{
				return TaskRunners<object, Task>.RunTask(task, () => successor(arg1));
			}

			internal static Task<Task> ThenWithArgs(Task task, Func<T1, T2, Task> successor, T1 arg1, T2 arg2)
			{
				return TaskRunners<object, Task>.RunTask(task, () => successor(arg1, arg2));
			}

			internal static Task<Task> ThenWithArgs(Task task, Func<T1, T2, T3, Task> successor, T1 arg1, T2 arg2, T3 arg3)
			{
				return TaskRunners<object, Task>.RunTask(task, () => successor(arg1, arg2, arg3));
			}

			internal static Task<Task<TResult>> ThenWithArgs(Task<T> task, Func<T, T1, Task<TResult>> successor, T1 arg1)
			{
				return TaskRunners<T, Task<TResult>>.RunTask(task, (Task<T> t) => successor(t.Result, arg1));
			}

			internal static Task<Task<T>> ThenWithArgs(Task<T> task, Func<Task<T>, T1, Task<T>> successor, T1 arg1)
			{
				return TaskRunners<T, Task<T>>.RunTask(task, (Task<T> t) => successor(t, arg1));
			}
		}

		private static class TaskCache<T>
		{
			public static Task<T> Empty = MakeTask(default(T));
		}

		private static readonly Task _emptyTask = MakeTask<object>(null);

		private static readonly Task<bool> _trueTask = MakeTask(true);

		private static readonly Task<bool> _falseTask = MakeTask(false);

		public static Task Empty => _emptyTask;

		public static Task<bool> True => _trueTask;

		public static Task<bool> False => _falseTask;

		private static Task<T> MakeTask<T>(T value)
		{
			return FromResult(value);
		}

		public static Task OrEmpty(this Task task)
		{
			return task ?? Empty;
		}

		public static Task<T> OrEmpty<T>(this Task<T> task)
		{
			return task ?? TaskCache<T>.Empty;
		}

		public static Task FromAsync(Func<AsyncCallback, object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod, object state)
		{
			try
			{
				return Task.Factory.FromAsync(beginMethod, endMethod, state);
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task<T> FromAsync<T>(Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, T> endMethod, object state)
		{
			try
			{
				return Task.Factory.FromAsync(beginMethod, endMethod, state);
			}
			catch (Exception e)
			{
				return FromError<T>(e);
			}
		}

		public static TTask Catch<TTask>(this TTask task, ILogger logger = null) where TTask : Task
		{
			return task.Catch(delegate
			{
			}, logger);
		}

		public static TTask Catch<TTask>(this TTask task, ILogger logger, params IPerformanceCounter[] counters) where TTask : Task
		{
			return task.Catch(delegate
			{
				if (counters != null)
				{
					for (int i = 0; i < counters.Length; i++)
					{
						counters[i].Increment();
					}
				}
			}, logger);
		}

		public static TTask Catch<TTask>(this TTask task, Action<AggregateException, object> handler, object state, ILogger logger = null) where TTask : Task
		{
			if (task != null && task.Status != TaskStatus.RanToCompletion)
			{
				if (task.Status == TaskStatus.Faulted)
				{
					ExecuteOnFaulted(handler, state, task.Exception, logger);
				}
				else
				{
					AttachFaultedContinuation(task, handler, state, logger);
				}
			}
			return task;
		}

		private static void AttachFaultedContinuation<TTask>(TTask task, Action<AggregateException, object> handler, object state, ILogger logger) where TTask : Task
		{
			task.ContinueWithPreservedCulture(delegate(Task innerTask)
			{
				ExecuteOnFaulted(handler, state, innerTask.Exception, logger);
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
		}

		private static void ExecuteOnFaulted(Action<AggregateException, object> handler, object state, AggregateException exception, ILogger logger)
		{
			logger?.LogWarning(0, exception, "Exception thrown by Task");
			handler(exception, state);
		}

		public static TTask Catch<TTask>(this TTask task, Action<AggregateException> handler, ILogger logger = null) where TTask : Task
		{
			return task.Catch(delegate(AggregateException ex, object state)
			{
				((Action<AggregateException>)state)(ex);
			}, handler, logger);
		}

		public static Task ContinueWithNotComplete(this Task task, Action action)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				try
				{
					action();
					return task;
				}
				catch (Exception e)
				{
					return FromError(e);
				}
			case TaskStatus.RanToCompletion:
				return task;
			default:
			{
				TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
				task.ContinueWithPreservedCulture(delegate(Task t)
				{
					if (t.IsFaulted || t.IsCanceled)
					{
						try
						{
							action();
							if (t.IsFaulted)
							{
								tcs.TrySetUnwrappedException(t.Exception);
							}
							else
							{
								tcs.TrySetCanceled();
							}
						}
						catch (Exception exception)
						{
							tcs.TrySetException(exception);
						}
					}
					else
					{
						tcs.TrySetResult(null);
					}
				}, TaskContinuationOptions.ExecuteSynchronously);
				return tcs.Task;
			}
			}
		}

		public static void ContinueWithNotComplete(this Task task, TaskCompletionSource<object> tcs)
		{
			task.ContinueWithPreservedCulture(delegate(Task t)
			{
				if (t.IsFaulted)
				{
					tcs.SetUnwrappedException(t.Exception);
				}
				else if (t.IsCanceled)
				{
					tcs.SetCanceled();
				}
			}, TaskContinuationOptions.NotOnRanToCompletion);
		}

		public static Task ContinueWith(this Task task, TaskCompletionSource<object> tcs)
		{
			task.ContinueWithPreservedCulture(delegate(Task t)
			{
				if (t.IsFaulted)
				{
					tcs.TrySetUnwrappedException(t.Exception);
				}
				else if (t.IsCanceled)
				{
					tcs.TrySetCanceled();
				}
				else
				{
					tcs.TrySetResult(null);
				}
			}, TaskContinuationOptions.ExecuteSynchronously);
			return tcs.Task;
		}

		public static void ContinueWith<T>(this Task<T> task, TaskCompletionSource<T> tcs)
		{
			task.ContinueWithPreservedCulture(delegate(Task<T> t)
			{
				if (t.IsFaulted)
				{
					tcs.TrySetUnwrappedException(t.Exception);
				}
				else if (t.IsCanceled)
				{
					tcs.TrySetCanceled();
				}
				else
				{
					tcs.TrySetResult(t.Result);
				}
			});
		}

		public static Task Then(this Task task, Action successor)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor);
			default:
				return RunTask(task, successor);
			}
		}

		public static Task<TResult> Then<TResult>(this Task task, Func<TResult> successor)
		{
			switch (task.Status)
			{
			case TaskStatus.Faulted:
				return FromError<TResult>(task.Exception);
			case TaskStatus.Canceled:
				return Canceled<TResult>();
			case TaskStatus.RanToCompletion:
				return FromMethod(successor);
			default:
				return TaskRunners<object, TResult>.RunTask(task, successor);
			}
		}

		public static Task Then<T1>(this Task task, Action<T1> successor, T1 arg1)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, arg1);
			default:
				return GenericDelegates<object, object, T1, object, object>.ThenWithArgs(task, successor, arg1);
			}
		}

		public static Task Then<T1, T2>(this Task task, Action<T1, T2> successor, T1 arg1, T2 arg2)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, arg1, arg2);
			default:
				return GenericDelegates<object, object, T1, T2, object>.ThenWithArgs(task, successor, arg1, arg2);
			}
		}

		public static Task Then<T1>(this Task task, Func<T1, Task> successor, T1 arg1)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, arg1);
			default:
				return GenericDelegates<object, Task, T1, object, object>.ThenWithArgs(task, successor, arg1).FastUnwrap();
			}
		}

		public static Task Then<T1, T2>(this Task task, Func<T1, T2, Task> successor, T1 arg1, T2 arg2)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, arg1, arg2);
			default:
				return GenericDelegates<object, Task, T1, T2, object>.ThenWithArgs(task, successor, arg1, arg2).FastUnwrap();
			}
		}

		public static Task Then<T1, T2, T3>(this Task task, Func<T1, T2, T3, Task> successor, T1 arg1, T2 arg2, T3 arg3)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, arg1, arg2, arg3);
			default:
				return GenericDelegates<object, Task, T1, T2, T3>.ThenWithArgs(task, successor, arg1, arg2, arg3).FastUnwrap();
			}
		}

		public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, Task<TResult>> successor)
		{
			switch (task.Status)
			{
			case TaskStatus.Faulted:
				return FromError<TResult>(task.Exception);
			case TaskStatus.Canceled:
				return Canceled<TResult>();
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, task.Result);
			default:
				return TaskRunners<T, Task<TResult>>.RunTask(task, (Task<T> t) => successor(t.Result)).FastUnwrap();
			}
		}

		public static Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, TResult> successor)
		{
			switch (task.Status)
			{
			case TaskStatus.Faulted:
				return FromError<TResult>(task.Exception);
			case TaskStatus.Canceled:
				return Canceled<TResult>();
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, task.Result);
			default:
				return TaskRunners<T, TResult>.RunTask(task, (Task<T> t) => successor(t.Result));
			}
		}

		public static Task<TResult> Then<T, T1, TResult>(this Task<T> task, Func<T, T1, TResult> successor, T1 arg1)
		{
			switch (task.Status)
			{
			case TaskStatus.Faulted:
				return FromError<TResult>(task.Exception);
			case TaskStatus.Canceled:
				return Canceled<TResult>();
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, task.Result, arg1);
			default:
				return GenericDelegates<T, TResult, T1, object, object>.ThenWithArgs(task, successor, arg1);
			}
		}

		public static Task Then(this Task task, Func<Task> successor)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor);
			default:
				return TaskRunners<object, Task>.RunTask(task, successor).FastUnwrap();
			}
		}

		public static Task<TResult> Then<TResult>(this Task task, Func<Task<TResult>> successor)
		{
			switch (task.Status)
			{
			case TaskStatus.Faulted:
				return FromError<TResult>(task.Exception);
			case TaskStatus.Canceled:
				return Canceled<TResult>();
			case TaskStatus.RanToCompletion:
				return FromMethod(successor);
			default:
				return TaskRunners<object, Task<TResult>>.RunTask(task, successor).FastUnwrap();
			}
		}

		public static Task Then<TResult>(this Task<TResult> task, Action<TResult> successor)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, task.Result);
			default:
				return TaskRunners<TResult, object>.RunTask(task, successor);
			}
		}

		public static Task Then<TResult>(this Task<TResult> task, Func<TResult, Task> successor)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, task.Result);
			default:
				return TaskRunners<TResult, Task>.RunTask(task, (Task<TResult> t) => successor(t.Result)).FastUnwrap();
			}
		}

		public static Task<TResult> Then<TResult, T1>(this Task<TResult> task, Func<Task<TResult>, T1, Task<TResult>> successor, T1 arg1)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor, task, arg1);
			default:
				return GenericDelegates<TResult, Task<TResult>, T1, object, object>.ThenWithArgs(task, successor, arg1).FastUnwrap();
			}
		}

		public static Task Finally(this Task task, Action<object> next, object state)
		{
			try
			{
				switch (task.Status)
				{
				case TaskStatus.Canceled:
				case TaskStatus.Faulted:
					next(state);
					return task;
				case TaskStatus.RanToCompletion:
					return FromMethod(next, state);
				default:
					return RunTaskSynchronously(task, next, state, false);
				}
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task RunSynchronously(this Task task, Action successor)
		{
			switch (task.Status)
			{
			case TaskStatus.Canceled:
			case TaskStatus.Faulted:
				return task;
			case TaskStatus.RanToCompletion:
				return FromMethod(successor);
			default:
				return RunTaskSynchronously(task, delegate(object state)
				{
					((Action)state)();
				}, successor);
			}
		}

		public static Task FastUnwrap(this Task<Task> task)
		{
			return ((task.Status == TaskStatus.RanToCompletion) ? task.Result : null) ?? task.Unwrap();
		}

		public static Task<T> FastUnwrap<T>(this Task<Task<T>> task)
		{
			return ((task.Status == TaskStatus.RanToCompletion) ? task.Result : null) ?? task.Unwrap();
		}

		public static Task Delay(TimeSpan timeOut)
		{
			return Task.Delay(timeOut);
		}

		public static Task FromMethod(Action func)
		{
			try
			{
				func();
				return Empty;
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task FromMethod<T1>(Action<T1> func, T1 arg)
		{
			try
			{
				func(arg);
				return Empty;
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task FromMethod<T1, T2>(Action<T1, T2> func, T1 arg1, T2 arg2)
		{
			try
			{
				func(arg1, arg2);
				return Empty;
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task FromMethod(Func<Task> func)
		{
			try
			{
				return func();
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task<TResult> FromMethod<TResult>(Func<Task<TResult>> func)
		{
			try
			{
				return func();
			}
			catch (Exception e)
			{
				return FromError<TResult>(e);
			}
		}

		public static Task<TResult> FromMethod<TResult>(Func<TResult> func)
		{
			try
			{
				return FromResult(func());
			}
			catch (Exception e)
			{
				return FromError<TResult>(e);
			}
		}

		public static Task FromMethod<T1>(Func<T1, Task> func, T1 arg)
		{
			try
			{
				return func(arg);
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task FromMethod<T1, T2>(Func<T1, T2, Task> func, T1 arg1, T2 arg2)
		{
			try
			{
				return func(arg1, arg2);
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task FromMethod<T1, T2, T3>(Func<T1, T2, T3, Task> func, T1 arg1, T2 arg2, T3 arg3)
		{
			try
			{
				return func(arg1, arg2, arg3);
			}
			catch (Exception e)
			{
				return FromError(e);
			}
		}

		public static Task<TResult> FromMethod<T1, TResult>(Func<T1, Task<TResult>> func, T1 arg)
		{
			try
			{
				return func(arg);
			}
			catch (Exception e)
			{
				return FromError<TResult>(e);
			}
		}

		public static Task<TResult> FromMethod<T1, TResult>(Func<T1, TResult> func, T1 arg)
		{
			try
			{
				return FromResult(func(arg));
			}
			catch (Exception e)
			{
				return FromError<TResult>(e);
			}
		}

		public static Task<TResult> FromMethod<T1, T2, TResult>(Func<T1, T2, Task<TResult>> func, T1 arg1, T2 arg2)
		{
			try
			{
				return func(arg1, arg2);
			}
			catch (Exception e)
			{
				return FromError<TResult>(e);
			}
		}

		public static Task<TResult> FromMethod<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 arg1, T2 arg2)
		{
			try
			{
				return FromResult(func(arg1, arg2));
			}
			catch (Exception e)
			{
				return FromError<TResult>(e);
			}
		}

		public static Task<T> FromResult<T>(T value)
		{
			TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
			taskCompletionSource.SetResult(value);
			return taskCompletionSource.Task;
		}

		internal static Task FromError(Exception e)
		{
			return FromError<object>(e);
		}

		internal static Task<T> FromError<T>(Exception e)
		{
			TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
			taskCompletionSource.SetUnwrappedException(e);
			return taskCompletionSource.Task;
		}

		internal static void SetUnwrappedException<T>(this TaskCompletionSource<T> tcs, Exception e)
		{
			AggregateException ex = e as AggregateException;
			if (ex != null)
			{
				tcs.SetException(ex.InnerExceptions);
			}
			else
			{
				tcs.SetException(e);
			}
		}

		internal static bool TrySetUnwrappedException<T>(this TaskCompletionSource<T> tcs, Exception e)
		{
			AggregateException ex = e as AggregateException;
			if (ex != null)
			{
				return tcs.TrySetException(ex.InnerExceptions);
			}
			return tcs.TrySetException(e);
		}

		private static Task Canceled()
		{
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetCanceled();
			return taskCompletionSource.Task;
		}

		private static Task<T> Canceled<T>()
		{
			TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
			taskCompletionSource.SetCanceled();
			return taskCompletionSource.Task;
		}

		internal static Task ContinueWithPreservedCulture(this Task task, Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
		{
			return task.ContinueWith(continuationAction, continuationOptions);
		}

		internal static Task ContinueWithPreservedCulture<T>(this Task<T> task, Action<Task<T>> continuationAction, TaskContinuationOptions continuationOptions)
		{
			return task.ContinueWith(continuationAction, continuationOptions);
		}

		internal static Task<TResult> ContinueWithPreservedCulture<T, TResult>(this Task<T> task, Func<Task<T>, TResult> continuationAction, TaskContinuationOptions continuationOptions)
		{
			return task.ContinueWith(continuationAction, continuationOptions);
		}

		internal static Task ContinueWithPreservedCulture(this Task task, Action<Task> continuationAction)
		{
			return task.ContinueWithPreservedCulture(continuationAction, TaskContinuationOptions.None);
		}

		internal static Task ContinueWithPreservedCulture<T>(this Task<T> task, Action<Task<T>> continuationAction)
		{
			return task.ContinueWithPreservedCulture(continuationAction, TaskContinuationOptions.None);
		}

		internal static Task<TResult> ContinueWithPreservedCulture<T, TResult>(this Task<T> task, Func<Task<T>, TResult> continuationAction)
		{
			return task.ContinueWithPreservedCulture(continuationAction, TaskContinuationOptions.None);
		}

		private static Task RunTask(Task task, Action successor)
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
			task.ContinueWithPreservedCulture(delegate(Task t)
			{
				if (t.IsFaulted)
				{
					tcs.SetUnwrappedException(t.Exception);
				}
				else if (t.IsCanceled)
				{
					tcs.SetCanceled();
				}
				else
				{
					try
					{
						successor();
						tcs.SetResult(null);
					}
					catch (Exception e)
					{
						tcs.SetUnwrappedException(e);
					}
				}
			});
			return tcs.Task;
		}

		private static Task RunTaskSynchronously(Task task, Action<object> next, object state, bool onlyOnSuccess = true)
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
			task.ContinueWithPreservedCulture(delegate(Task t)
			{
				try
				{
					if (t.IsFaulted)
					{
						if (!onlyOnSuccess)
						{
							next(state);
						}
						tcs.SetUnwrappedException(t.Exception);
					}
					else if (t.IsCanceled)
					{
						if (!onlyOnSuccess)
						{
							next(state);
						}
						tcs.SetCanceled();
					}
					else
					{
						next(state);
						tcs.SetResult(null);
					}
				}
				catch (Exception e)
				{
					tcs.SetUnwrappedException(e);
				}
			}, TaskContinuationOptions.ExecuteSynchronously);
			return tcs.Task;
		}
	}
}
