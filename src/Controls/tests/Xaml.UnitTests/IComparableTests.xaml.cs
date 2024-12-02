using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class IComparableTests : ContentPage
	{
		public List<string> P { get; set; }

		public IComparableTests()
		{
			InitializeComponent();
		}

		public IComparableTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[Test]
			public void TestXamlCErrorWithIComparable()
			{
				var xaml = 
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			 xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			 xmlns:library="clr-namespace:Microsoft.Maui.Controls.ControlGallery;assembly=Microsoft.Maui.Controls.ControlGallery"
			 x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.IComparableTests">
	
    <ContentPage.Resources>
        <x:String x:Key="ComparingValue">1.0</x:String>
    </ContentPage.Resources>

    <VerticalStackLayout
        Padding="30,0"
        Spacing="25">
        <Label Text="Updating" />
        <BoxView />

        <ActivityIndicator
            IsRunning="{Binding UpdateProgress, Converter={library:CompareConverter ComparingValue={StaticResource ComparingValue}, ComparisonOperator=Smaller, FalseObject=False, TrueObject=True}}" />

        <Label Text="{Binding Message}" />
    </VerticalStackLayout>

</ContentPage>
""";

				var contentPage = new ContentPage();
				contentPage.LoadFromXaml(xaml);

				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(ContentPage)));
			}
		}
	}
}