using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal class HubMethodDispatcher
	{
		private delegate object HubMethodExecutor(IHub hub, object[] parameters);

		private delegate void VoidHubMethodExecutor(IHub hub, object[] parameters);

		private HubMethodExecutor _executor;

		public MethodInfo MethodInfo
		{
			get;
			private set;
		}

		public HubMethodDispatcher(MethodInfo methodInfo)
		{
			_executor = GetExecutor(methodInfo);
			MethodInfo = methodInfo;
		}

		public object Execute(IHub hub, object[] parameters)
		{
			return _executor(hub, parameters);
		}

		private static HubMethodExecutor GetExecutor(MethodInfo methodInfo)
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(IHub), "hub");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object[]), "parameters");
			List<Expression> list = new List<Expression>();
			ParameterInfo[] parameters = methodInfo.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameterInfo = parameters[i];
				UnaryExpression item = Expression.Convert(Expression.ArrayIndex(parameterExpression2, Expression.Constant(i)), parameterInfo.ParameterType);
				list.Add(item);
			}
			MethodCallExpression methodCallExpression = Expression.Call((!methodInfo.IsStatic) ? Expression.Convert(parameterExpression, methodInfo.DeclaringType) : null, methodInfo, list);
			if ((object)methodCallExpression.Type == typeof(void))
			{
				return WrapVoidAction(Expression.Lambda<VoidHubMethodExecutor>(methodCallExpression, new ParameterExpression[2]
				{
					parameterExpression,
					parameterExpression2
				}).Compile());
			}
			return Expression.Lambda<HubMethodExecutor>(Expression.Convert(methodCallExpression, typeof(object)), new ParameterExpression[2]
			{
				parameterExpression,
				parameterExpression2
			}).Compile();
		}

		private static HubMethodExecutor WrapVoidAction(VoidHubMethodExecutor executor)
		{
			return delegate(IHub hub, object[] parameters)
			{
				executor(hub, parameters);
				return null;
			};
		}
	}
}
