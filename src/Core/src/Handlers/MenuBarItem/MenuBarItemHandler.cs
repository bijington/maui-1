﻿using System;
using System.Collections.Generic;
using System.Text;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIMenu;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuBarItem;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : IMenuBarItemHandler
	{

		public static IPropertyMapper<IMenuBarItem, IMenuBarItemHandler> Mapper = new PropertyMapper<IMenuBarItem, IMenuBarItemHandler>(ElementMapper)
		{
#if WINDOWS
			[nameof(IMenuBarItem.Text)] = MapText,
#endif
		};

		public static CommandMapper<IMenuBarItem, IMenuBarItemHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IMenuBarItemHandler.Add)] = MapAdd,
			[nameof(IMenuBarItemHandler.Remove)] = MapRemove,
			[nameof(IMenuBarItemHandler.Clear)] = MapClear,
			[nameof(IMenuBarItemHandler.Insert)] = MapInsert,
		};

		public MenuBarItemHandler() : base(Mapper, CommandMapper)
		{

		}

		public static void MapAdd(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
			{
				handler.Add(args.MenuElement);
			}
		}

		public static void MapRemove(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
			{
				handler.Remove(args.MenuElement);
			}
		}

		public static void MapInsert(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			if (arg is MenuBarItemHandlerUpdate args)
			{
				handler.Insert(args.Index, args.MenuElement);
			}
		}

		public static void MapClear(IMenuBarItemHandler handler, IMenuBarItem layout, object? arg)
		{
			handler.Clear();
		}

		IMenuBarItem IMenuBarItemHandler.VirtualView => VirtualView;

		PlatformView IMenuBarItemHandler.PlatformView => PlatformView;
	}
}
