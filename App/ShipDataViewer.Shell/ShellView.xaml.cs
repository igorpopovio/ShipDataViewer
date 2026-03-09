using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ShipDataViewer.Shell;

public partial class ShellView
{
	public ShellViewModel ViewModel => (ShellViewModel)DataContext;

	public ShellView()
	{
		InitializeComponent();
	}

	private void Window_Loaded(object sender, RoutedEventArgs args)
	{
		CollectionViewSource.GetDefaultView(ShipDataGrid.ItemsSource).Filter = ViewModel.Filter;
	}

	private void TextBox_TextChanged(object sender, TextChangedEventArgs args)
	{
		CollectionViewSource.GetDefaultView(ShipDataGrid.ItemsSource).Refresh();
	}
}
