Azure Notification Hubs SDK for Silverlight 8.1 using WNS
=========================================================

# What's this?

According to the official website, Notification Hubs Windows Phone SDK does not support using WNS with Windows Phone 8.1 Silverlight apps. To use WNS (instead of MPNS) with Windows Phone 8.1 Silverlight apps, you have to set up your WNS credentials as shown in [Get Started for Windows Universal](http://azure.microsoft.com/en-us/documentation/articles/notification-hubs-windows-store-dotnet-get-started/). Then, you can register from the back-end as shown in the [Notify Users](http://azure.microsoft.com/en-us/documentation/articles/notification-hubs-aspnet-backend-windows-dotnet-notify-users/) tutorial, or use the [Notification Hubs REST APIs](http://msdn.microsoft.com/en-us/library/dn223264.aspx).

To provide an easy way to use WNS with Windows Phone 8.1 Silverlight apps, this is an unofficial Azure Notification Hubs SDK for Silverlight 8.1 app using Windows Notification Service (WNS).

Currently, this SDK is still under development, lack of some advanced feature.

# Usage

Create a NotificationHub client:

```
NotificationHub hub = new NotificationHub(NOTIFICATION_HUB_NAME, CONNECTION_STRING);
```

Request a push channel and register for it:

```
PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
await hub.RegisterNativeAsync(channel.Uri);
```

or you can register with tags:

```
List<string> tags = new List<string>();
tags.Add("tester");
await hub.RegisterNativeAsync(channel.Uri, tags);
```

# Todo

* Secondary tile channel support
* Exception handling