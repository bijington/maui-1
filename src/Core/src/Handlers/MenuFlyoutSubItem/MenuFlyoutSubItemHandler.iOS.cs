﻿using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using ObjCRuntime;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSubItemHandler
	{
		protected override UIMenu CreatePlatformElement()
		{
			UIMenuElement[] menuElements = new UIMenuElement[VirtualView.Count];
			for (int i = 0; i < VirtualView.Count; i++)
			{
				var item = VirtualView[i];
				var menuElement = (UIMenuElement)item.ToHandler(MauiContext!)!.PlatformView!;
				menuElements[i] = menuElement;
			}

			//var selector = new Selector(Guid.NewGuid().ToString());
			//var command = UICommand.Create(title: VirtualView.Text, null, selector, null);
			var menu = UIMenu.Create(VirtualView.Text, menuElements);
			return menu;
		}

		[Export("MenuFlyoutSubItemHandlerMenuClickAction:")]
		public void MenuClickAction(UICommand uICommand)
		{

		}

		public void Add(IMenuElement view)
		{
		}

		public void Remove(IMenuElement view)
		{
		}

		public void Clear()
		{
		}

		public void Insert(int index, IMenuElement view)
		{
		}
	}
}
