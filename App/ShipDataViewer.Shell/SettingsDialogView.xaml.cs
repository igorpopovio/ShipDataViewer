using System.Windows;

namespace ShipDataViewer.Shell;

public partial class SettingsDialogView
{
	public SettingsDialogViewModel ViewModel => (SettingsDialogViewModel)DataContext;

	public SettingsDialogView()
	{
		InitializeComponent();
	}

	private void FluentWindow_Loaded(object sender, RoutedEventArgs args)
	{
		ViewModel.Close += (s, e) => Close();
	}
}
