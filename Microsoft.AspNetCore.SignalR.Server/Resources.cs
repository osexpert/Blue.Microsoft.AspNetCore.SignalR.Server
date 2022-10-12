using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.SignalR.Server
{
	[DebuggerNonUserCode]
	[CompilerGenerated]
	public class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("Microsoft.AspNetCore.SignalR.Server.Resources", typeof(Resources).GetTypeInfo().Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		public static string DynamicComment_CallsMethodOnServerSideDeferredPromise => ResourceManager.GetString("DynamicComment_CallsMethodOnServerSideDeferredPromise", resourceCulture);

		public static string DynamicComment_ServerSideTypeIs => ResourceManager.GetString("DynamicComment_ServerSideTypeIs", resourceCulture);

		public static string Error_AmbiguousMessage => ResourceManager.GetString("Error_AmbiguousMessage", resourceCulture);

		public static string Error_ArgumentNullOrEmpty => ResourceManager.GetString("Error_ArgumentNullOrEmpty", resourceCulture);

		public static string Error_BufferSizeOutOfRange => ResourceManager.GetString("Error_BufferSizeOutOfRange", resourceCulture);

		public static string Error_CallerNotAuthorizedToInvokeMethodOn => ResourceManager.GetString("Error_CallerNotAuthorizedToInvokeMethodOn", resourceCulture);

		public static string Error_ConnectionIdIncorrectFormat => ResourceManager.GetString("Error_ConnectionIdIncorrectFormat", resourceCulture);

		public static string Error_ConnectionNotInitialized => ResourceManager.GetString("Error_ConnectionNotInitialized", resourceCulture);

		public static string Error_DisconnectTimeoutCannotBeConfiguredAfterKeepAlive => ResourceManager.GetString("Error_DisconnectTimeoutCannotBeConfiguredAfterKeepAlive", resourceCulture);

		public static string Error_DisconnectTimeoutMustBeAtLeastSixSeconds => ResourceManager.GetString("Error_DisconnectTimeoutMustBeAtLeastSixSeconds", resourceCulture);

		public static string Error_DoNotReadRequireOutgoing => ResourceManager.GetString("Error_DoNotReadRequireOutgoing", resourceCulture);

		public static string Error_DuplicateHubNames => ResourceManager.GetString("Error_DuplicateHubNames", resourceCulture);

		public static string Error_DuplicateHubNamesInConnectionData => ResourceManager.GetString("Error_DuplicateHubNamesInConnectionData", resourceCulture);

		public static string Error_DuplicatePayloadsForStream => ResourceManager.GetString("Error_DuplicatePayloadsForStream", resourceCulture);

		public static string Error_ExceptionContextCanOnlyBeModifiedOnce => ResourceManager.GetString("Error_ExceptionContextCanOnlyBeModifiedOnce", resourceCulture);

		public static string Error_HubCouldNotBeResolved => ResourceManager.GetString("Error_HubCouldNotBeResolved", resourceCulture);

		public static string Error_HubInvocationFailed => ResourceManager.GetString("Error_HubInvocationFailed", resourceCulture);

		public static string Error_HubProgressOnlyReportableBeforeMethodReturns => ResourceManager.GetString("Error_HubProgressOnlyReportableBeforeMethodReturns", resourceCulture);

		public static string Error_InvalidCursorFormat => ResourceManager.GetString("Error_InvalidCursorFormat", resourceCulture);

		public static string Error_IsNotA => ResourceManager.GetString("Error_IsNotA", resourceCulture);

		public static string Error_JavaScriptProxyDisabled => ResourceManager.GetString("Error_JavaScriptProxyDisabled", resourceCulture);

		public static string Error_KeepAliveMustBeGreaterThanTwoSeconds => ResourceManager.GetString("Error_KeepAliveMustBeGreaterThanTwoSeconds", resourceCulture);

		public static string Error_KeepAliveMustBeNoMoreThanAThirdOfTheDisconnectTimeout => ResourceManager.GetString("Error_KeepAliveMustBeNoMoreThanAThirdOfTheDisconnectTimeout", resourceCulture);

		public static string Error_MethodCouldNotBeResolved => ResourceManager.GetString("Error_MethodCouldNotBeResolved", resourceCulture);

		public static string Error_MethodCouldNotBeResolvedCandidates => ResourceManager.GetString("Error_MethodCouldNotBeResolvedCandidates", resourceCulture);

		public static string Error_MethodLevelOutgoingAuthorization => ResourceManager.GetString("Error_MethodLevelOutgoingAuthorization", resourceCulture);

		public static string Error_MethodMustNotTakeOutParameter => ResourceManager.GetString("Error_MethodMustNotTakeOutParameter", resourceCulture);

		public static string Error_MethodMustNotTakeRefParameter => ResourceManager.GetString("Error_MethodMustNotTakeRefParameter", resourceCulture);

		public static string Error_MethodMustReturnVoidOrTask => ResourceManager.GetString("Error_MethodMustReturnVoidOrTask", resourceCulture);

		public static string Error_MultipleActivatorsAreaRegisteredCallGetServices => ResourceManager.GetString("Error_MultipleActivatorsAreaRegisteredCallGetServices", resourceCulture);

		public static string Error_NoConfiguration => ResourceManager.GetString("Error_NoConfiguration", resourceCulture);

		public static string Error_NoDependencyResolver => ResourceManager.GetString("Error_NoDependencyResolver", resourceCulture);

		public static string Error_NoTransportsEnabled => ResourceManager.GetString("Error_NoTransportsEnabled", resourceCulture);

		public static string Error_NotWebSocketRequest => ResourceManager.GetString("Error_NotWebSocketRequest", resourceCulture);

		public static string Error_ParseObjectFailed => ResourceManager.GetString("Error_ParseObjectFailed", resourceCulture);

		public static string Error_ProtocolErrorMissingConnectionToken => ResourceManager.GetString("Error_ProtocolErrorMissingConnectionToken", resourceCulture);

		public static string Error_ProtocolErrorUnknownTransport => ResourceManager.GetString("Error_ProtocolErrorUnknownTransport", resourceCulture);

		public static string Error_ScaleoutQueuingConfig => ResourceManager.GetString("Error_ScaleoutQueuingConfig", resourceCulture);

		public static string Error_ServicesNotRegistered => ResourceManager.GetString("Error_ServicesNotRegistered", resourceCulture);

		public static string Error_StreamClosed => ResourceManager.GetString("Error_StreamClosed", resourceCulture);

		public static string Error_StreamNotOpen => ResourceManager.GetString("Error_StreamNotOpen", resourceCulture);

		public static string Error_TaskQueueFull => ResourceManager.GetString("Error_TaskQueueFull", resourceCulture);

		public static string Error_TypeMustBeInterface => ResourceManager.GetString("Error_TypeMustBeInterface", resourceCulture);

		public static string Error_TypeMustNotContainEvents => ResourceManager.GetString("Error_TypeMustNotContainEvents", resourceCulture);

		public static string Error_TypeMustNotContainProperties => ResourceManager.GetString("Error_TypeMustNotContainProperties", resourceCulture);

		public static string Error_UnableToAddModulePiplineAlreadyInvoked => ResourceManager.GetString("Error_UnableToAddModulePiplineAlreadyInvoked", resourceCulture);

		public static string Error_UnrecognizedUserIdentity => ResourceManager.GetString("Error_UnrecognizedUserIdentity", resourceCulture);

		public static string Error_UsingHubInstanceNotCreatedUnsupported => ResourceManager.GetString("Error_UsingHubInstanceNotCreatedUnsupported", resourceCulture);

		public static string Error_WebSocketsNotSupported => ResourceManager.GetString("Error_WebSocketsNotSupported", resourceCulture);

		public static string Forbidden_JSONPDisabled => ResourceManager.GetString("Forbidden_JSONPDisabled", resourceCulture);

		internal Resources()
		{
		}
	}
}
