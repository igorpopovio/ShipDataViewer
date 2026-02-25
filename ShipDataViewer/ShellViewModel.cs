using Stylet;

namespace ShipDataViewer;

public class ShellViewModel : Screen
{
	private readonly string DefaultLoadingMessage = "Ready";

	private readonly IWindowManager _windowManager;

	public string Details { get; set; } = string.Empty;
	public string LoadingMessage { get; set; }

	public ShellViewModel(IWindowManager windowManager)
	{
		DisplayName = "Ship Data Viewer";
		LoadingMessage = DefaultLoadingMessage;
		_windowManager = windowManager;
	}

	public async Task LoadShipDetails()
	{
		LoadingMessage = "Connecting to server...";
		await Task.Delay(1000);
		Details = "Data loaded";
		LoadingMessage = "Data loaded successfully!";
		await Task.Delay(2000);
		LoadingMessage = DefaultLoadingMessage;
	}
}
