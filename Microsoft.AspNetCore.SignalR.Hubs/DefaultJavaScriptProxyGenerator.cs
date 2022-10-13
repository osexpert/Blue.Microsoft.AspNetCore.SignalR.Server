using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.SignalR.Json;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Hubs
{
	public class DefaultJavaScriptProxyGenerator : IJavaScriptProxyGenerator
	{
		private static readonly Lazy<string> _templateFromResource = new Lazy<string>(GetTemplateFromResource);

		private static readonly Type[] _numberTypes = new Type[7]
		{
			typeof(byte),
			typeof(short),
			typeof(int),
			typeof(long),
			typeof(float),
			typeof(decimal),
			typeof(double)
		};

		private static readonly Type[] _dateTypes = new Type[2]
		{
			typeof(DateTime),
			typeof(DateTimeOffset)
		};

        //		private static readonly string ScriptResource = typeof(DefaultJavaScriptProxyGenerator).GetTypeInfo().Assembly.GetName()
        //			.Name + ".Scripts.hubs.js";
        private static readonly string ScriptResource = "Microsoft.AspNetCore.SignalR.Server" + ".Scripts.hubs.js";
       

        private readonly IHubManager _manager;

		private readonly IJavaScriptMinifier _javaScriptMinifier;

		private readonly Lazy<string> _generatedTemplate;

		public DefaultJavaScriptProxyGenerator(IHubManager manager, IJavaScriptMinifier javaScriptMinifier)
		{
			_manager = manager;
			_javaScriptMinifier = javaScriptMinifier;
			_generatedTemplate = new Lazy<string>(() => GenerateProxy(_manager, _javaScriptMinifier, false));
		}

		public string GenerateProxy(string serviceUrl)
		{
			serviceUrl = JavaScriptEncode(serviceUrl);
			return _generatedTemplate.Value.Replace("{serviceUrl}", serviceUrl);
		}

		public string GenerateProxy(string serviceUrl, bool includeDocComments)
		{
			serviceUrl = JavaScriptEncode(serviceUrl);
			return GenerateProxy(_manager, _javaScriptMinifier, includeDocComments).Replace("{serviceUrl}", serviceUrl);
		}

		private static string GenerateProxy(IHubManager hubManager, IJavaScriptMinifier javaScriptMinifier, bool includeDocComments)
		{
			string value = _templateFromResource.Value;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (HubDescriptor item in from h in hubManager.GetHubs()
				orderby h.Name
				select h)
			{
				if (!flag)
				{
					stringBuilder.AppendLine(";");
					stringBuilder.AppendLine();
					stringBuilder.Append("    ");
				}
				GenerateType(hubManager, stringBuilder, item, includeDocComments);
				flag = false;
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(";");
			}
			value = value.Replace("/*hubs*/", stringBuilder.ToString());
			return javaScriptMinifier.Minify(value);
		}

		private static void GenerateType(IHubManager hubManager, StringBuilder sb, HubDescriptor descriptor, bool includeDocComments)
		{
			IEnumerable<MethodDescriptor> methods = GetMethods(hubManager, descriptor);
			string descriptorName = GetDescriptorName(descriptor);
			sb.AppendFormat("    proxies['{0}'] = this.createHubProxy('{1}'); ", descriptorName, descriptorName).AppendLine();
			sb.AppendFormat("        proxies['{0}'].client = {{ }};", descriptorName).AppendLine();
			sb.AppendFormat("        proxies['{0}'].server = {{", descriptorName);
			bool flag = true;
			foreach (MethodDescriptor item in methods)
			{
				if (!flag)
				{
					sb.Append(",").AppendLine();
				}
				GenerateMethod(sb, item, includeDocComments, descriptorName);
				flag = false;
			}
			sb.AppendLine();
			sb.Append("        }");
		}

		private static string GetDescriptorName(Descriptor descriptor)
		{
			if (descriptor == null)
			{
				throw new ArgumentNullException("descriptor");
			}
			string text = descriptor.Name;
			if (!descriptor.NameSpecified)
			{
				text = JsonUtility.CamelCase(text);
			}
			return text;
		}

		private static IEnumerable<MethodDescriptor> GetMethods(IHubManager manager, HubDescriptor descriptor)
		{
			return from method in manager.GetHubMethods(descriptor.Name)
				group method by method.Name into overloads
				let oload = overloads.OrderBy((MethodDescriptor overload) => overload.Parameters.Count).FirstOrDefault()
				orderby oload.Name
				select oload;
		}

		private static void GenerateMethod(StringBuilder sb, MethodDescriptor method, bool includeDocComments, string hubName)
		{
			List<string> values = method.Parameters.Select((ParameterDescriptor p) => p.Name).ToList();
			sb.AppendLine();
			sb.AppendFormat("            {0}: function ({1}) {{", GetDescriptorName(method), Commas(values)).AppendLine();
			if (includeDocComments)
			{
				sb.AppendFormat(Resources.DynamicComment_CallsMethodOnServerSideDeferredPromise, method.Name, method.Hub.Name).AppendLine();
				List<string> list = method.Parameters.Select((ParameterDescriptor p) => string.Format(CultureInfo.CurrentCulture, Resources.DynamicComment_ServerSideTypeIs, p.Name, MapToJavaScriptType(p.ParameterType), p.ParameterType)).ToList();
				if (list.Any())
				{
					sb.AppendLine(string.Join(Environment.NewLine, list));
				}
			}
			sb.AppendFormat("                return proxies['{0}'].invoke.apply(proxies['{0}'], $.merge([\"{1}\"], $.makeArray(arguments)));", hubName, method.Name).AppendLine();
			sb.Append("             }");
		}

		private static string MapToJavaScriptType(Type type)
		{
			if (!type.GetTypeInfo().IsPrimitive && (object)type != typeof(string))
			{
				return "Object";
			}
			if ((object)type == typeof(string))
			{
				return "String";
			}
			if (_numberTypes.Contains(type))
			{
				return "Number";
			}
			if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
			{
				return "Array";
			}
			if (_dateTypes.Contains(type))
			{
				return "Date";
			}
			return string.Empty;
		}

		private static string Commas(IEnumerable<string> values)
		{
			return Commas(values, (string v) => v);
		}

		private static string Commas<T>(IEnumerable<T> values, Func<T, string> selector)
		{
			return string.Join(", ", values.Select(selector));
		}

		private static string GetTemplateFromResource()
		{
			using (Stream stream = typeof(DefaultJavaScriptProxyGenerator).GetTypeInfo().Assembly.GetManifestResourceStream(ScriptResource))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}

		private static string JavaScriptEncode(string value)
		{
			value = JsonConvert.SerializeObject(value);
			return value.Substring(1, value.Length - 2);
		}
	}
}
