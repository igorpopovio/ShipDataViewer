namespace ShipDataViewer.Core;

public interface IService
{
	Task Listen(CancellationToken token = default);
}
