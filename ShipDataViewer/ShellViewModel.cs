using Stylet;

namespace ShipDataViewer;

public class ShellViewModel : Screen
{
	private readonly IWindowManager _windowManager;

	public string NameString { get; set; }

	public ShellViewModel(IWindowManager windowManager)
	{
		DisplayName = "Hello Dialog";
		NameString = "Test string";
		_windowManager = windowManager;
	}

	public void ShowDialog()
	{
		//bool? result = this._windowManager.ShowDialog(dialogVm);
		//if (result.GetValueOrDefault())
		//	this.NameString = $"Your name is {dialogVm.Name}";
		//else
		//	this.NameString = "Dialog cancelled";
	}
}
