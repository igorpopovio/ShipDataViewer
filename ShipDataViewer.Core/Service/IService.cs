namespace ShipDataViewer.Core.Service;

public interface IService
{
	Task ListenAsync(CancellationToken token = default);
}
