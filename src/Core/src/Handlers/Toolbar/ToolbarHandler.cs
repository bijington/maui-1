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
	public partial class ToolbarHandler : IToolbarHandler
	{
		public static IPropertyMapper<IToolbar, IToolbarHandler> Mapper =
			   new PropertyMapper<IToolbar, IToolbarHandler>(ElementMapper)
			   {
				   [nameof(IToolbar.Title)] = MapTitle,
			   };

		public static CommandMapper<IToolbar, IToolbarHandler> CommandMapper = new();

		public ToolbarHandler() : base(Mapper, CommandMapper)
		{
		}

		IToolbar IToolbarHandler.VirtualView => VirtualView;
		PlatformView IToolbarHandler.PlatformView => PlatformView;
	}
}
