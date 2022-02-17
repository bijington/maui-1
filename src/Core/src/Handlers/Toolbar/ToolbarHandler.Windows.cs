﻿using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, MauiToolbar>
	{
		protected override MauiToolbar CreatePlatformElement()
		{
			return new MauiToolbar();
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
			arg1.PlatformView.UpdateTitle(arg2);
		}
	}
}
