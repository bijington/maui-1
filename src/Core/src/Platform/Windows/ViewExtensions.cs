#nullable enable
using System;
using System.Numerics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WFlowDirection = Microsoft.UI.Xaml.FlowDirection;
using WinPoint = Windows.Foundation.Point;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Platform
{
	public static partial class ViewExtensions
	{
		static Canvas? BackgroundLayer;

		public static void TryMoveFocus(this FrameworkElement platformView, FocusNavigationDirection direction)
		{
			if (platformView?.XamlRoot?.Content is UIElement elem)
				FocusManager.TryMoveFocus(direction, new FindNextElementOptions { SearchRoot = elem });
		}

		public static void UpdateIsEnabled(this FrameworkElement platformView, IView view) =>
			(platformView as Control)?.UpdateIsEnabled(view.IsEnabled);

		public static void UpdateInputTransparent(this FrameworkElement platformView, IView view)
		{
			var panel = platformView as Panel;

			if (NeedsBackgroundLayer(view))
			{
				platformView.IsHitTestVisible = true;

				if (panel !=null)
					panel.AddBackgroundLayer(view);
			}
			else
			{
				if (panel != null)
					panel.RemoveBackgroundLayer(view);

				platformView.IsHitTestVisible = view.IsEnabled && !view.InputTransparent;

				if (!platformView.IsHitTestVisible)
				{
					return;
				}

				// If this Panel's background brush is null, the UWP considers it transparent to hit testing (even 
				// when IsHitTestVisible is true). So we have to explicitly set a background brush to make it show up
				// in hit testing. 
				if (view is ILayout && panel != null && panel.Background == null)
				{
					panel.Background = new SolidColorBrush(UI.Colors.Transparent);
				}
			}
		}

		internal static bool NeedsBackgroundLayer(IView view)
		{
			if (view is not ILayout layout)
			{
				return false;
			}

			if (layout.IsEnabled && layout.InputTransparent && !layout.CascadeInputTransparent)
			{
				return true;
			}

			return false;
		}

		internal static void AddBackgroundLayer(this Panel nativeControl, IView view)
		{
			if (BackgroundLayer != null)
			{
				return;
			}

			// In WinUI, once a control has hit testing disabled, all of its child controls
			// also have hit testing disabled. The exception is a Panel with its 
			// Background Brush set to `null`; the Panel will be invisible to hit testing, but its
			// children will work just fine. 

			// In order to handle the situation where we need the layout to be invisible to hit testing,
			// the child controls to be visible to hit testing, *and* we need to support non-null
			// background brushes, we insert another empty Panel which is invisible to hit testing; that
			// Panel will be our Background color

			BackgroundLayer = new Canvas { IsHitTestVisible = false };
			nativeControl.Children.Insert(0, BackgroundLayer);

			nativeControl.UpdateBackground(view);
		}

		internal static void RemoveBackgroundLayer(this Panel nativeControl, IView view)
		{
			if (BackgroundLayer == null)
			{
				return;
			}

			nativeControl.Children.Remove(BackgroundLayer);
			BackgroundLayer = null;

			nativeControl.UpdateBackground(view);
		}

		public static void UpdateVisibility(this FrameworkElement platformView, IView view)
		{
			double opacity = view.Opacity;
			var wasCollapsed = platformView.Visibility == UI.Xaml.Visibility.Collapsed;

			switch (view.Visibility)
			{
				case Visibility.Visible:
					platformView.Opacity = opacity;
					platformView.Visibility = UI.Xaml.Visibility.Visible;
					break;
				case Visibility.Hidden:
					platformView.Opacity = 0;
					platformView.Visibility = UI.Xaml.Visibility.Visible;
					break;
				case Visibility.Collapsed:
					platformView.Opacity = opacity;
					platformView.Visibility = UI.Xaml.Visibility.Collapsed;
					break;
			}

			if (view.Visibility != Visibility.Collapsed && wasCollapsed)
			{
				// We may need to force the parent layout (if any) to re-layout to accomodate the new size
				(platformView.Parent as FrameworkElement)?.InvalidateMeasure();
			}
		}

		public static void UpdateClip(this FrameworkElement platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
			{
				wrapper.Clip = view.Clip;
			}
		}

		public static void UpdateShadow(this FrameworkElement platformView, IView view)
		{
			if (platformView is WrapperView wrapper)
			{
				wrapper.Shadow = view.Shadow;
			}
		}

		public static void UpdateBorder(this FrameworkElement platformView, IView view)
		{
			var border = (view as IBorder)?.Border;
			if (platformView is WrapperView wrapperView)
				wrapperView.Border = border;
		}

		public static void UpdateOpacity(this FrameworkElement platformView, IView view)
		{
			platformView.Opacity = view.Visibility == Visibility.Hidden ? 0 : view.Opacity;
		}

		public static void UpdateBackground(this ContentPanel platformView, IBorderStroke border) 
		{
			var hasBorder = border.Shape != null && border.Stroke != null;

			if (hasBorder)
			{
				platformView?.UpdateBorderBackground(border);
			}
			else if(border is IView v)
			{
				platformView?.UpdatePlatformViewBackground(v);
			}
		}

		public static void UpdateBackground(this FrameworkElement platformView, IView view)
		{
			platformView?.UpdatePlatformViewBackground(view);
		}

		public static WFlowDirection ToPlatform(this FlowDirection flowDirection)
		{
			if (flowDirection == FlowDirection.RightToLeft)
				return WFlowDirection.RightToLeft;
			else if (flowDirection == FlowDirection.LeftToRight)
				return WFlowDirection.LeftToRight;

			throw new InvalidOperationException($"Invalid FlowDirection: {flowDirection}");
		}

		public static void UpdateFlowDirection(this FrameworkElement platformView, IView view)
		{
			var flowDirection = view.FlowDirection;

			if (flowDirection == FlowDirection.MatchParent ||
				view.FlowDirection == FlowDirection.MatchParent)
			{
				flowDirection = view?.Handler?.MauiContext?.GetFlowDirection()
					?? FlowDirection.LeftToRight;
			}
			if (flowDirection == FlowDirection.MatchParent)
			{
				flowDirection = FlowDirection.LeftToRight;
			}

			platformView.FlowDirection = flowDirection.ToPlatform();
		}

		public static void UpdateAutomationId(this FrameworkElement platformView, IView view) =>
			AutomationProperties.SetAutomationId(platformView, view.AutomationId);

		public static void UpdateSemantics(this FrameworkElement platformView, IView view)
		{
			var semantics = view.Semantics;

			if (semantics == null)
				return;

			AutomationProperties.SetName(platformView, semantics.Description);
			AutomationProperties.SetHelpText(platformView, semantics.Hint);
			AutomationProperties.SetHeadingLevel(platformView, (UI.Xaml.Automation.Peers.AutomationHeadingLevel)((int)semantics.HeadingLevel));
		}

		internal static void UpdateProperty(this FrameworkElement platformControl, DependencyProperty property, Color color)
		{
			if (color.IsDefault())
				platformControl.ClearValue(property);
			else
				platformControl.SetValue(property, color.ToPlatform());
		}

		internal static void UpdateProperty(this FrameworkElement platformControl, DependencyProperty property, object? value)
		{
			if (value == null)
				platformControl.ClearValue(property);
			else
				platformControl.SetValue(property, value);
		}

		public static void InvalidateMeasure(this FrameworkElement platformView, IView view)
		{
			platformView.InvalidateMeasure();
		}

		public static void UpdateWidth(this FrameworkElement platformView, IView view)
		{
			// WinUI uses NaN for "unspecified", so as long as we're using NaN for unspecified on the xplat side, 
			// we can just propagate the value straight through
			platformView.Width = view.Width;
		}

		public static void UpdateHeight(this FrameworkElement platformView, IView view)
		{
			// WinUI uses NaN for "unspecified", so as long as we're using NaN for unspecified on the xplat side, 
			// we can just propagate the value straight through
			platformView.Height = view.Height;
		}

		public static void UpdateMinimumHeight(this FrameworkElement platformView, IView view)
		{
			var minHeight = view.MinimumHeight;

			if (Dimension.IsMinimumSet(minHeight))
			{
				// We only use the minimum value if it's been explicitly set; otherwise, leave it alone
				// because the platform/theme may have a minimum height for this control
				platformView.MinHeight = minHeight;
			}
		}

		public static void UpdateMinimumWidth(this FrameworkElement platformView, IView view)
		{
			var minWidth = view.MinimumWidth;

			if (Dimension.IsMinimumSet(minWidth))
			{
				// We only use the minimum value if it's been explicitly set; otherwise, leave it alone
				// because the platform/theme may have a minimum width for this control
				platformView.MinWidth = minWidth;
			}
		}

		public static void UpdateMaximumHeight(this FrameworkElement platformView, IView view)
		{
			platformView.MaxHeight = view.MaximumHeight;
		}

		public static void UpdateMaximumWidth(this FrameworkElement platformView, IView view)
		{
			platformView.MaxWidth = view.MaximumWidth;
		}

		internal static void UpdateBorderBackground(this FrameworkElement platformView, IBorderStroke border)
		{

			if(border is IView v)
			(platformView as ContentPanel)?.UpdateBackground(v.Background);

			if (platformView is Control control)
				control.UpdateBackground((Paint?)null);
			else if (platformView is Border b)
				b.UpdateBackground(null);
			else if (platformView is Panel panel)
				panel.UpdateBackground(null);
		}

		internal static void UpdatePlatformViewBackground(this FrameworkElement platformView, IView view)
		{
			(platformView as ContentPanel)?.UpdateBackground(null);

			if (platformView is Control control)
				control.UpdateBackground(view.Background);
			else if (platformView is Border border)
				border.UpdateBackground(view.Background);
			else if (platformView is Panel panel)
				panel.UpdateBackground(view.Background);
		}

		public static async Task<byte[]?> RenderAsPNG(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return null;

			return await platformView.RenderAsPNG();
		}

		public static async Task<byte[]?> RenderAsJPEG(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return null;

			return await platformView.RenderAsJPEG();
		}

		public static Task<byte[]?> RenderAsPNG(this FrameworkElement view) =>  Task.FromResult<byte[]?>(null);

		public static Task<byte[]?> RenderAsJPEG(this FrameworkElement view) =>  Task.FromResult<byte[]?>(null);

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView == null)
				return new Matrix4x4();
			return GetViewTransform(platformView);
		}

		internal static Matrix4x4 GetViewTransform(this FrameworkElement element)
		{
			var root = element?.Parent as UIElement;
			if (root == null)
				return new Matrix4x4();
			var offset = element?.TransformToVisual(root) as MatrixTransform;
			if (offset == null)
				return new Matrix4x4();
			Matrix matrix = offset.Matrix;
			return new Matrix4x4()
			{
				M11 = (float)matrix.M11,
				M12 = (float)matrix.M12,
				M21 = (float)matrix.M21,
				M22 = (float)matrix.M22,
				Translation = new Vector3((float)matrix.OffsetX, (float)matrix.OffsetY, 0)
			};
		}

		internal static Rectangle GetPlatformViewBounds(this IView view)
		{
			var platformView = view?.ToPlatform();
			if (platformView != null)
				return platformView.GetPlatformViewBounds();
			return new Rectangle();
		}

		internal static Rectangle GetPlatformViewBounds(this FrameworkElement platformView)
		{
			if (platformView == null)
				return new Rectangle();

			var root = platformView.XamlRoot;
			var offset = platformView.TransformToVisual(root.Content) as UI.Xaml.Media.MatrixTransform;
			if (offset != null)
				return new Rectangle(offset.Matrix.OffsetX, offset.Matrix.OffsetY, platformView.ActualWidth, platformView.ActualHeight);

			return new Rectangle();
		}

		internal static Graphics.Rectangle GetBoundingBox(this IView view) 
			=> view.ToPlatform().GetBoundingBox();

		internal static Graphics.Rectangle GetBoundingBox(this FrameworkElement? platformView)
		{
			if (platformView == null)
				return new Rectangle();

			var rootView = platformView.XamlRoot.Content;
			if (platformView == rootView)
			{
				if (rootView is not FrameworkElement el)
					return new Rectangle();

				return new Rectangle(0, 0, el.ActualWidth, el.ActualHeight);
			}


			var topLeft = platformView.TransformToVisual(rootView).TransformPoint(new WinPoint());
			var topRight = platformView.TransformToVisual(rootView).TransformPoint(new WinPoint(platformView.ActualWidth, 0));
			var bottomLeft = platformView.TransformToVisual(rootView).TransformPoint(new WinPoint(0, platformView.ActualHeight));
			var bottomRight = platformView.TransformToVisual(rootView).TransformPoint(new WinPoint(platformView.ActualWidth, platformView.ActualHeight));


			var x1 = new[] { topLeft.X, topRight.X, bottomLeft.X, bottomRight.X }.Min();
			var x2 = new[] { topLeft.X, topRight.X, bottomLeft.X, bottomRight.X }.Max();
			var y1 = new[] { topLeft.Y, topRight.Y, bottomLeft.Y, bottomRight.Y }.Min();
			var y2 = new[] { topLeft.Y, topRight.Y, bottomLeft.Y, bottomRight.Y }.Max();
			return new Rectangle(x1, y1, x2 - x1, y2 - y1);
		}

		internal static DependencyObject? GetParent(this FrameworkElement? view)
		{
			return view?.Parent;
		}

		internal static DependencyObject? GetParent(this DependencyObject? view)
		{
			if (view is FrameworkElement pv)
				return pv.Parent;

			return null;
		}

		internal static IWindow? GetHostedWindow(this IView? view)
			=> GetHostedWindow(view?.Handler?.PlatformView as FrameworkElement);

		internal static IWindow? GetHostedWindow(this FrameworkElement? view)
			=> GetWindowForXamlRoot(view?.XamlRoot);

		internal static IWindow? GetWindowForXamlRoot(XamlRoot? root)
		{
			if (root is null)
				return null;

			var windows = WindowExtensions.GetWindows();
			foreach(var window in windows)
			{
				if (window.Handler?.PlatformView is Microsoft.UI.Xaml.Window win)
				{
					if (win.Content?.XamlRoot == root)
						return window;
				}
			}

			return null;
		}
	}
}
