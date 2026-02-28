using ShipDataViewer.Core.Model;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ShipDataViewer.Ui;

public partial class ShellView
{
	public ShellViewModel? ViewModel => DataContext as ShellViewModel;

	public ShellView()
	{
		InitializeComponent();
	}

	private void CollectionViewSource_Filter(object sender, FilterEventArgs args)
	{
		if (ViewModel == null || string.IsNullOrWhiteSpace(ViewModel.FilterText))
		{
			args.Accepted = true;
			return;
		}

		if (args.Item is Ship ship)
		{
			args.Accepted = ship.Name.Contains(ViewModel.FilterText, StringComparison.OrdinalIgnoreCase);
		}
		else
		{
			args.Accepted = false;
		}
	}

	private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
	{
		CollectionViewSource.GetDefaultView(ShipDataGrid.ItemsSource).Filter = UserFilter;
	}

	private bool UserFilter(object obj)
	{
		if (ViewModel == null)
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(ViewModel.FilterText))
		{
			return true;
		}

		if (obj is Ship ship)
		{
			return ship.Name.Contains(ViewModel.FilterText, StringComparison.OrdinalIgnoreCase);
		}

		return false;
	}

	private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		CollectionViewSource.GetDefaultView(ShipDataGrid.ItemsSource).Refresh();
	}
}
