using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using System.Threading.Channels;

namespace RandomlyGeneratedAisStream.Service;

public class RandomlyGeneratedAiStreamService : IService
{
	private readonly Channel<Ship> _shipData;
	private readonly Channel<Position> _positionData;

	public ChannelReader<Ship> ShipData => _shipData.Reader;

	public ChannelReader<Position> PositionData => _positionData.Reader;

	public RandomlyGeneratedAiStreamService()
	{
		_shipData = Channel.CreateUnbounded<Ship>();
		_positionData = Channel.CreateUnbounded<Position>();
	}

	public async Task ListenAsync(CancellationToken token = default)
	{
		var ships = new List<Ship>();
		for (int i = 0; i < 50 && !token.IsCancellationRequested; i++)
		{
			var ship = new Ship
			{
				Mmsi = i,
				ImoNumber = i,
				CallSign = $"Call Sign {i}",
				Name = $"Ship{i}",
			};
			ships.Add(ship);
			await _shipData.Writer.WriteAsync(ship, token).ConfigureAwait(false);
		}

		var random = new Random();
		while (!token.IsCancellationRequested)
		{
			foreach (var ship in ships)
			{
				var position = new Position
				{
					ShipMmsi = ship.Mmsi,
					Latitude = -90 + random.NextDouble() * 180,
					Longitude = -180 + random.NextDouble() * 360,
					Sog = random.NextDouble(),
					Cog = random.NextDouble(),
					TrueHeading = random.Next(),
				};
				await _positionData.Writer.WriteAsync(position, token).ConfigureAwait(false);
			}

			await Task.Delay(TimeSpan.FromMilliseconds(500), token);
		}
	}
}
