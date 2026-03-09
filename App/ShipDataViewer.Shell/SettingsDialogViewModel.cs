using Stylet;

namespace ShipDataViewer.Shell;

public class SettingsDialogViewModel : Screen
{
	public string? ApiKey { get; set; }

	public void Save()
	{
		RequestClose(true);
	}

	public void Cancel()
	{
		RequestClose(false);
	}
}
