using ShipDataViewer.Core.Model;

namespace ShipDataViewer.Shell;

public abstract class ViewModel : Model
{
	public event EventHandler<EventArgs>? Close;

	protected virtual void OnClose()
	{
		Close?.Invoke(this, EventArgs.Empty);
	}
}
