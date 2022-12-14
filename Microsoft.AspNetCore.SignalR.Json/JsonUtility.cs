using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.SignalR.Json
{
	public static class JsonUtility
	{
		private const int DefaultMaxDepth = 20;

		private static readonly string[] _jsKeywords = new string[45]
		{
			"break",
			"do",
			"instanceof",
			"typeof",
			"case",
			"else",
			"new",
			"var",
			"catch",
			"finally",
			"return",
			"void",
			"continue",
			"for",
			"switch",
			"while",
			"debugger",
			"function",
			"this",
			"with",
			"default",
			"if",
			"throw",
			"delete",
			"in",
			"try",
			"class",
			"enum",
			"extends",
			"super",
			"const",
			"export",
			"import",
			"implements",
			"let",
			"private",
			"public",
			"yield",
			"interface",
			"package",
			"protected",
			"static",
			"NaN",
			"undefined",
			"Infinity"
		};

		public static string JsonMimeType => "application/json; charset=UTF-8";

		public static string JavaScriptMimeType => "application/javascript; charset=UTF-8";

		public static string CamelCase(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return string.Join(".", from n in name.Split(new char[1]
				{
					'.'
				})
				select char.ToLowerInvariant(n[0]) + n.Substring(1));
		}

		public static string CreateJsonpCallback(string callback, string payload)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!IsValidJavaScriptCallback(callback))
			{
				throw new InvalidOperationException();
			}
			stringBuilder.AppendFormat("{0}(", callback).Append(payload).Append(");");
			return stringBuilder.ToString();
		}

		public static JsonSerializerSettings CreateDefaultSerializerSettings()
		{
			return new JsonSerializerSettings
			{
				MaxDepth = 20
			};
		}

		public static JsonSerializer CreateDefaultSerializer()
		{
			return JsonSerializer.Create(CreateDefaultSerializerSettings());
		}

		internal static bool IsValidJavaScriptCallback(string callback)
		{
			if (string.IsNullOrWhiteSpace(callback))
			{
				return false;
			}
			string[] array = callback.Split(new char[1]
			{
				'.'
			});
			for (int i = 0; i < array.Length; i++)
			{
				if (!IsValidJavaScriptFunctionName(array[i]))
				{
					return false;
				}
			}
			return true;
		}

		internal static bool IsValidJavaScriptFunctionName(string name)
		{
			if (string.IsNullOrWhiteSpace(name) || IsJavaScriptReservedWord(name))
			{
				return false;
			}
			if (!IsValidJavaScriptIdentifierStartChar(name[0]))
			{
				return false;
			}
			for (int i = 1; i < name.Length; i++)
			{
				if (!IsValidJavaScriptIdenfitierNonStartChar(name[i]))
				{
					return false;
				}
			}
			return true;
		}

		internal static bool TryRejectJSONPRequest(SignalROptions options, HttpContext context)
		{
			if (options.EnableJSONP)
			{
				return false;
			}
			if (string.IsNullOrEmpty(context.Request.Query["callback"]))
			{
				return false;
			}
			context.Response.StatusCode = 403;
			return true;
		}

		private static bool IsValidJavaScriptIdentifierStartChar(char startChar)
		{
			if (!char.IsLetter(startChar) && startChar != '$')
			{
				return startChar == '_';
			}
			return true;
		}

		private static bool IsValidJavaScriptIdenfitierNonStartChar(char identifierChar)
		{
			if (!char.IsLetterOrDigit(identifierChar) && identifierChar != '$')
			{
				return identifierChar == '_';
			}
			return true;
		}

		private static bool IsJavaScriptReservedWord(string word)
		{
			return _jsKeywords.Contains(word);
		}
	}
}
