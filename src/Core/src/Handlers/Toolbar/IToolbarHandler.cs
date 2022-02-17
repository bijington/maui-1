﻿#if IOS || MACCATALYST
using PlatformView = UIKit.UINavigationBar;
#elif MONOANDROID
using PlatformView = Google.Android.Material.AppBar.MaterialToolbar;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.MauiToolbar;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IToolbarHandler : IElementHandler
	{
		new IToolbar VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}