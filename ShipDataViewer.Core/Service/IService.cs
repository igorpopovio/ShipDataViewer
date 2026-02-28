using ShipDataViewer.Core.Model;

namespace ShipDataViewer.Core.Service;

public interface IService
{
	Task ListenAsync(CancellationToken token = default);

	event EventHandler<Ship> ShipDataReceived;
}
