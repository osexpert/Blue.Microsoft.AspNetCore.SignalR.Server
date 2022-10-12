using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	internal static class TypedClientBuilder<T>
	{
		private const string ClientModuleName = "Microsoft.AspNetCore.SignalR.Hubs.TypedClientBuilder";

		private static Lazy<Func<IClientProxy, T>> _builder = new Lazy<Func<IClientProxy, T>>(() => GenerateClientBuilder());

		public static T Build(IClientProxy proxy)
		{
			return _builder.Value(proxy);
		}

		public static void Validate()
		{
			Func<IClientProxy, T> value = _builder.Value;
		}

		private static Func<IClientProxy, T> GenerateClientBuilder()
		{
			VerifyInterface(typeof(T));
			ModuleBuilder moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Microsoft.AspNetCore.SignalR.Hubs.TypedClientBuilder"), AssemblyBuilderAccess.Run).DefineDynamicModule("Microsoft.AspNetCore.SignalR.Hubs.TypedClientBuilder");
			Type clientType = GenerateInterfaceImplementation(moduleBuilder);
			return (IClientProxy proxy) => (T)Activator.CreateInstance(clientType, proxy);
		}

		private static Type GenerateInterfaceImplementation(ModuleBuilder moduleBuilder)
		{
			TypeBuilder typeBuilder = moduleBuilder.DefineType("Microsoft.AspNetCore.SignalR.Hubs.TypedClientBuilder." + typeof(T).get_Name() + "Impl", TypeAttributes.Public, typeof(object), new Type[1]
			{
				typeof(T)
			});
			FieldBuilder proxyField = typeBuilder.DefineField("_proxy", typeof(IClientProxy), FieldAttributes.Private);
			BuildConstructor(typeBuilder, proxyField);
			foreach (MethodInfo allInterfaceMethod in GetAllInterfaceMethods(typeof(T)))
			{
				BuildMethod(typeBuilder, allInterfaceMethod, proxyField);
			}
			return typeBuilder.CreateTypeInfo().AsType();
		}

		private static IEnumerable<MethodInfo> GetAllInterfaceMethods(Type interfaceType)
		{
			Type[] interfaces = TypeExtensions.GetInterfaces(interfaceType);
			foreach (Type interfaceType2 in interfaces)
			{
				foreach (MethodInfo allInterfaceMethod in GetAllInterfaceMethods(interfaceType2))
				{
					yield return allInterfaceMethod;
				}
			}
			MethodInfo[] methods = TypeExtensions.GetMethods(interfaceType);
			foreach (MethodInfo methodInfo in methods)
			{
				yield return methodInfo;
			}
		}

		private static void BuildConstructor(TypeBuilder type, FieldInfo proxyField)
		{
			MethodBuilder methodBuilder = type.DefineMethod(".ctor", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig);
			ConstructorInfo con = typeof(object).GetTypeInfo().DeclaredConstructors.First((ConstructorInfo c) => c.GetParameters().Length == 0);
			methodBuilder.SetReturnType(typeof(void));
			methodBuilder.SetParameters(typeof(IClientProxy));
			ILGenerator iLGenerator = methodBuilder.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, con);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Stfld, proxyField);
			iLGenerator.Emit(OpCodes.Ret);
		}

		private static void BuildMethod(TypeBuilder type, MethodInfo interfaceMethodInfo, FieldInfo proxyField)
		{
			MethodAttributes attributes = MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask;
			ParameterInfo[] parameters = interfaceMethodInfo.GetParameters();
			Type[] array = (from param in parameters
			select param.ParameterType).ToArray();
			MethodBuilder methodBuilder = type.DefineMethod(interfaceMethodInfo.Name, attributes);
			MethodInfo runtimeMethod = RuntimeReflectionExtensions.GetRuntimeMethod(typeof(IClientProxy), "Invoke", new Type[2]
			{
				typeof(string),
				typeof(object[])
			});
			methodBuilder.SetReturnType(interfaceMethodInfo.ReturnType);
			methodBuilder.SetParameters(array);
			string[] array2 = (from p in array
			where p.IsGenericParameter
			select p.get_Name()).Distinct().ToArray();
			if (array2.Any())
			{
				methodBuilder.DefineGenericParameters(array2);
			}
			ILGenerator iLGenerator = methodBuilder.GetILGenerator();
			iLGenerator.DeclareLocal(typeof(object[]));
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, proxyField);
			iLGenerator.Emit(OpCodes.Ldstr, interfaceMethodInfo.Name);
			iLGenerator.Emit(OpCodes.Ldc_I4, parameters.Length);
			iLGenerator.Emit(OpCodes.Newarr, typeof(object));
			iLGenerator.Emit(OpCodes.Stloc_0);
			for (int i = 0; i < array.Length; i++)
			{
				iLGenerator.Emit(OpCodes.Ldloc_0);
				iLGenerator.Emit(OpCodes.Ldc_I4, i);
				iLGenerator.Emit(OpCodes.Ldarg, i + 1);
				iLGenerator.Emit(OpCodes.Box, array[i]);
				iLGenerator.Emit(OpCodes.Stelem_Ref);
			}
			iLGenerator.Emit(OpCodes.Ldloc_0);
			iLGenerator.Emit(OpCodes.Callvirt, runtimeMethod);
			if ((object)interfaceMethodInfo.ReturnType == typeof(void))
			{
				iLGenerator.Emit(OpCodes.Pop);
			}
			iLGenerator.Emit(OpCodes.Ret);
		}

		private static void VerifyInterface(Type interfaceType)
		{
			if (!interfaceType.GetTypeInfo().get_IsInterface())
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_TypeMustBeInterface, interfaceType.get_Name()));
			}
			if (TypeExtensions.GetProperties(interfaceType).Length != 0)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_TypeMustNotContainProperties, interfaceType.get_Name()));
			}
			if (TypeExtensions.GetEvents(interfaceType).Length != 0)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_TypeMustNotContainEvents, interfaceType.get_Name()));
			}
			MethodInfo[] methods = TypeExtensions.GetMethods(interfaceType);
			foreach (MethodInfo interfaceMethod in methods)
			{
				VerifyMethod(interfaceType, interfaceMethod);
			}
			Type[] interfaces = TypeExtensions.GetInterfaces(interfaceType);
			for (int i = 0; i < interfaces.Length; i++)
			{
				VerifyInterface(interfaces[i]);
			}
		}

		private static void VerifyMethod(Type interfaceType, MethodInfo interfaceMethod)
		{
			if ((object)interfaceMethod.ReturnType != typeof(void) && (object)interfaceMethod.ReturnType != typeof(Task))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_MethodMustReturnVoidOrTask, interfaceType.get_Name(), interfaceMethod.Name));
			}
			ParameterInfo[] parameters = interfaceMethod.GetParameters();
			foreach (ParameterInfo parameter in parameters)
			{
				VerifyParameter(interfaceType, interfaceMethod, parameter);
			}
		}

		private static void VerifyParameter(Type interfaceType, MethodInfo interfaceMethod, ParameterInfo parameter)
		{
			if (parameter.IsOut)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_MethodMustNotTakeOutParameter, parameter.Name, interfaceType.get_Name(), interfaceMethod.Name));
			}
			if (parameter.ParameterType.IsByRef)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_MethodMustNotTakeRefParameter, parameter.Name, interfaceType.get_Name(), interfaceMethod.Name));
			}
		}
	}
}
