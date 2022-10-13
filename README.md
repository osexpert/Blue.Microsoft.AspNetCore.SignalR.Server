# Blue.Microsoft.AspNetCore.SignalR.Server
based on Microsoft.AspNetCore.SignalR.Server 0.2.0-preview2-22683

Saved code from ILSpy.  
Changes some calls from Write to WriteAsync (HttpResponseStream?) since Write now trigger an error unless Kestrel AllowSynchronousIO is set to false.  

Also changed target to Netstandard 2.0  

The thing with this version of SignalR is that it is compatible with the AspNet Client. Later in the development of AspNetCore SignalR, compability was broken with AspNet Client. Specifically PersistentConnection was rempoved.


