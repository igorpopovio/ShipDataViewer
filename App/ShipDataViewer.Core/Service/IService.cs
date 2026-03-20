using ShipDataViewer.Core.Model;

using System.Threading.Channels;

namespace ShipDataViewer.Core.Service;

public interface IService
{
	Task ListenAsync(CancellationToken token = default);

	ChannelReader<Ship> ShipData { get; }

	ChannelReader<Position> PositionData { get; }
}
