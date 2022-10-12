using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Microsoft.AspNetCore.SignalR
{
	internal static class Resources
	{
		private static readonly ResourceManager _resourceManager = new ResourceManager("Microsoft.AspNetCore.SignalR.Server.Resources", typeof(Resources).GetTypeInfo().get_Assembly());

		internal static string DynamicComment_CallsMethodOnServerSideDeferredPromise => GetString("DynamicComment_CallsMethodOnServerSideDeferredPromise");

		internal static string DynamicComment_ServerSideTypeIs => GetString("DynamicComment_ServerSideTypeIs");

		internal static string Error_AmbiguousMessage => GetString("Error_AmbiguousMessage");

		internal static string Error_ArgumentNullOrEmpty => GetString("Error_ArgumentNullOrEmpty");

		internal static string Error_BufferSizeOutOfRange => GetString("Error_BufferSizeOutOfRange");

		internal static string Error_CallerNotAuthorizedToInvokeMethodOn => GetString("Error_CallerNotAuthorizedToInvokeMethodOn");

		internal static string Error_ConnectionIdIncorrectFormat => GetString("Error_ConnectionIdIncorrectFormat");

		internal static string Error_ConnectionNotInitialized => GetString("Error_ConnectionNotInitialized");

		internal static string Error_DisconnectTimeoutCannotBeConfiguredAfterKeepAlive => GetString("Error_DisconnectTimeoutCannotBeConfiguredAfterKeepAlive");

		internal static string Error_DisconnectTimeoutMustBeAtLeastSixSeconds => GetString("Error_DisconnectTimeoutMustBeAtLeastSixSeconds");

		internal static string Error_DoNotReadRequireOutgoing => GetString("Error_DoNotReadRequireOutgoing");

		internal static string Error_DuplicateHubNames => GetString("Error_DuplicateHubNames");

		internal static string Error_DuplicateHubNamesInConnectionData => GetString("Error_DuplicateHubNamesInConnectionData");

		internal static string Error_DuplicatePayloadsForStream => GetString("Error_DuplicatePayloadsForStream");

		internal static string Error_ExceptionContextCanOnlyBeModifiedOnce => GetString("Error_ExceptionContextCanOnlyBeModifiedOnce");

		internal static string Error_HubCouldNotBeResolved => GetString("Error_HubCouldNotBeResolved");

		internal static string Error_HubInvocationFailed => GetString("Error_HubInvocationFailed");

		internal static string Error_HubProgressOnlyReportableBeforeMethodReturns => GetString("Error_HubProgressOnlyReportableBeforeMethodReturns");

		internal static string Error_InvalidCursorFormat => GetString("Error_InvalidCursorFormat");

		internal static string Error_IsNotA => GetString("Error_IsNotA");

		internal static string Error_JavaScriptProxyDisabled => GetString("Error_JavaScriptProxyDisabled");

		internal static string Error_KeepAliveMustBeGreaterThanTwoSeconds => GetString("Error_KeepAliveMustBeGreaterThanTwoSeconds");

		internal static string Error_KeepAliveMustBeNoMoreThanAThirdOfTheDisconnectTimeout => GetString("Error_KeepAliveMustBeNoMoreThanAThirdOfTheDisconnectTimeout");

		internal static string Error_MethodCouldNotBeResolved => GetString("Error_MethodCouldNotBeResolved");

		internal static string Error_MethodCouldNotBeResolvedCandidates => GetString("Error_MethodCouldNotBeResolvedCandidates");

		internal static string Error_MethodLevelOutgoingAuthorization => GetString("Error_MethodLevelOutgoingAuthorization");

		internal static string Error_MethodMustNotTakeOutParameter => GetString("Error_MethodMustNotTakeOutParameter");

		internal static string Error_MethodMustNotTakeRefParameter => GetString("Error_MethodMustNotTakeRefParameter");

		internal static string Error_MethodMustReturnVoidOrTask => GetString("Error_MethodMustReturnVoidOrTask");

		internal static string Error_MultipleActivatorsAreaRegisteredCallGetServices => GetString("Error_MultipleActivatorsAreaRegisteredCallGetServices");

		internal static string Error_NoConfiguration => GetString("Error_NoConfiguration");

		internal static string Error_NoDependencyResolver => GetString("Error_NoDependencyResolver");

		internal static string Error_NoTransportsEnabled => GetString("Error_NoTransportsEnabled");

		internal static string Error_NotWebSocketRequest => GetString("Error_NotWebSocketRequest");

		internal static string Error_ParseObjectFailed => GetString("Error_ParseObjectFailed");

		internal static string Error_ProtocolErrorMissingConnectionToken => GetString("Error_ProtocolErrorMissingConnectionToken");

		internal static string Error_ProtocolErrorUnknownTransport => GetString("Error_ProtocolErrorUnknownTransport");

		internal static string Error_ScaleoutQueuingConfig => GetString("Error_ScaleoutQueuingConfig");

		internal static string Error_StateExceededMaximumLength => GetString("Error_StateExceededMaximumLength");

		internal static string Error_StreamClosed => GetString("Error_StreamClosed");

		internal static string Error_StreamNotOpen => GetString("Error_StreamNotOpen");

		internal static string Error_TaskQueueFull => GetString("Error_TaskQueueFull");

		internal static string Error_TypeMustBeInterface => GetString("Error_TypeMustBeInterface");

		internal static string Error_TypeMustNotContainEvents => GetString("Error_TypeMustNotContainEvents");

		internal static string Error_TypeMustNotContainProperties => GetString("Error_TypeMustNotContainProperties");

		internal static string Error_UnableToAddModulePiplineAlreadyInvoked => GetString("Error_UnableToAddModulePiplineAlreadyInvoked");

		internal static string Error_UnrecognizedUserIdentity => GetString("Error_UnrecognizedUserIdentity");

		internal static string Error_UsingHubInstanceNotCreatedUnsupported => GetString("Error_UsingHubInstanceNotCreatedUnsupported");

		internal static string Error_WebSocketsNotSupported => GetString("Error_WebSocketsNotSupported");

		internal static string Forbidden_JSONPDisabled => GetString("Forbidden_JSONPDisabled");

		internal static string Error_ServicesNotRegistered => GetString("Error_ServicesNotRegistered");

		internal static string FormatDynamicComment_CallsMethodOnServerSideDeferredPromise(object p0, object p1)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("DynamicComment_CallsMethodOnServerSideDeferredPromise"), p0, p1);
		}

		internal static string FormatDynamicComment_ServerSideTypeIs(object p0, object p1, object p2)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("DynamicComment_ServerSideTypeIs"), p0, p1, p2);
		}

		internal static string FormatError_AmbiguousMessage(object p0, object p1)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_AmbiguousMessage"), p0, p1);
		}

		internal static string FormatError_ArgumentNullOrEmpty()
		{
			return GetString("Error_ArgumentNullOrEmpty");
		}

		internal static string FormatError_BufferSizeOutOfRange(object p0)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_BufferSizeOutOfRange"), p0);
		}

		internal static string FormatError_CallerNotAuthorizedToInvokeMethodOn(object p0, object p1)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_CallerNotAuthorizedToInvokeMethodOn"), p0, p1);
		}

		internal static string FormatError_ConnectionIdIncorrectFormat()
		{
			return GetString("Error_ConnectionIdIncorrectFormat");
		}

		internal static string FormatError_ConnectionNotInitialized()
		{
			return GetString("Error_ConnectionNotInitialized");
		}

		internal static string FormatError_DisconnectTimeoutCannotBeConfiguredAfterKeepAlive()
		{
			return GetString("Error_DisconnectTimeoutCannotBeConfiguredAfterKeepAlive");
		}

		internal static string FormatError_DisconnectTimeoutMustBeAtLeastSixSeconds()
		{
			return GetString("Error_DisconnectTimeoutMustBeAtLeastSixSeconds");
		}

		internal static string FormatError_DoNotReadRequireOutgoing()
		{
			return GetString("Error_DoNotReadRequireOutgoing");
		}

		internal static string FormatError_DuplicateHubNames(object p0, object p1, object p2)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_DuplicateHubNames"), p0, p1, p2);
		}

		internal static string FormatError_DuplicateHubNamesInConnectionData()
		{
			return GetString("Error_DuplicateHubNamesInConnectionData");
		}

		internal static string FormatError_DuplicatePayloadsForStream(object p0)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_DuplicatePayloadsForStream"), p0);
		}

		internal static string FormatError_ExceptionContextCanOnlyBeModifiedOnce()
		{
			return GetString("Error_ExceptionContextCanOnlyBeModifiedOnce");
		}

		internal static string FormatError_HubCouldNotBeResolved(object p0)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_HubCouldNotBeResolved"), p0);
		}

		internal static string FormatError_HubInvocationFailed(object p0, object p1)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_HubInvocationFailed"), p0, p1);
		}

		internal static string FormatError_HubProgressOnlyReportableBeforeMethodReturns()
		{
			return GetString("Error_HubProgressOnlyReportableBeforeMethodReturns");
		}

		internal static string FormatError_InvalidCursorFormat()
		{
			return GetString("Error_InvalidCursorFormat");
		}

		internal static string FormatError_IsNotA(object p0, object p1)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_IsNotA"), p0, p1);
		}

		internal static string FormatError_JavaScriptProxyDisabled()
		{
			return GetString("Error_JavaScriptProxyDisabled");
		}

		internal static string FormatError_KeepAliveMustBeGreaterThanTwoSeconds()
		{
			return GetString("Error_KeepAliveMustBeGreaterThanTwoSeconds");
		}

		internal static string FormatError_KeepAliveMustBeNoMoreThanAThirdOfTheDisconnectTimeout()
		{
			return GetString("Error_KeepAliveMustBeNoMoreThanAThirdOfTheDisconnectTimeout");
		}

		internal static string FormatError_MethodCouldNotBeResolved(object p0)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_MethodCouldNotBeResolved"), p0);
		}

		internal static string FormatError_MethodCouldNotBeResolvedCandidates(object p0, object p1)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_MethodCouldNotBeResolvedCandidates"), p0, p1);
		}

		internal static string FormatError_MethodLevelOutgoingAuthorization()
		{
			return GetString("Error_MethodLevelOutgoingAuthorization");
		}

		internal static string FormatError_MethodMustNotTakeOutParameter(object p0, object p1, object p2)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_MethodMustNotTakeOutParameter"), p0, p1, p2);
		}

		internal static string FormatError_MethodMustNotTakeRefParameter(object p0, object p1, object p2)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_MethodMustNotTakeRefParameter"), p0, p1, p2);
		}

		internal static string FormatError_MethodMustReturnVoidOrTask(object p0, object p1)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_MethodMustReturnVoidOrTask"), p0, p1);
		}

		internal static string FormatError_MultipleActivatorsAreaRegisteredCallGetServices(object p0)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_MultipleActivatorsAreaRegisteredCallGetServices"), p0);
		}

		internal static string FormatError_NoConfiguration()
		{
			return GetString("Error_NoConfiguration");
		}

		internal static string FormatError_NoDependencyResolver()
		{
			return GetString("Error_NoDependencyResolver");
		}

		internal static string FormatError_NoTransportsEnabled()
		{
			return GetString("Error_NoTransportsEnabled");
		}

		internal static string FormatError_NotWebSocketRequest()
		{
			return GetString("Error_NotWebSocketRequest");
		}

		internal static string FormatError_ParseObjectFailed()
		{
			return GetString("Error_ParseObjectFailed");
		}

		internal static string FormatError_ProtocolErrorMissingConnectionToken()
		{
			return GetString("Error_ProtocolErrorMissingConnectionToken");
		}

		internal static string FormatError_ProtocolErrorUnknownTransport()
		{
			return GetString("Error_ProtocolErrorUnknownTransport");
		}

		internal static string FormatError_ScaleoutQueuingConfig()
		{
			return GetString("Error_ScaleoutQueuingConfig");
		}

		internal static string FormatError_StateExceededMaximumLength()
		{
			return GetString("Error_StateExceededMaximumLength");
		}

		internal static string FormatError_StreamClosed()
		{
			return GetString("Error_StreamClosed");
		}

		internal static string FormatError_StreamNotOpen()
		{
			return GetString("Error_StreamNotOpen");
		}

		internal static string FormatError_TaskQueueFull()
		{
			return GetString("Error_TaskQueueFull");
		}

		internal static string FormatError_TypeMustBeInterface(object p0)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_TypeMustBeInterface"), p0);
		}

		internal static string FormatError_TypeMustNotContainEvents(object p0)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_TypeMustNotContainEvents"), p0);
		}

		internal static string FormatError_TypeMustNotContainProperties(object p0)
		{
			return string.Format(CultureInfo.CurrentCulture, GetString("Error_TypeMustNotContainProperties"), p0);
		}

		internal static string FormatError_UnableToAddModulePiplineAlreadyInvoked()
		{
			return GetString("Error_UnableToAddModulePiplineAlreadyInvoked");
		}

		internal static string FormatError_UnrecognizedUserIdentity()
		{
			return GetString("Error_UnrecognizedUserIdentity");
		}

		internal static string FormatError_UsingHubInstanceNotCreatedUnsupported()
		{
			return GetString("Error_UsingHubInstanceNotCreatedUnsupported");
		}

		internal static string FormatError_WebSocketsNotSupported()
		{
			return GetString("Error_WebSocketsNotSupported");
		}

		internal static string FormatForbidden_JSONPDisabled()
		{
			return GetString("Forbidden_JSONPDisabled");
		}

		internal static string FormatError_ServicesNotRegistered()
		{
			return GetString("Error_ServicesNotRegistered");
		}

		private static string GetString(string name, params string[] formatterNames)
		{
			string text = _resourceManager.GetString(name);
			if (formatterNames != null)
			{
				for (int i = 0; i < formatterNames.Length; i++)
				{
					text = text.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
				}
			}
			return text;
		}
	}
}
