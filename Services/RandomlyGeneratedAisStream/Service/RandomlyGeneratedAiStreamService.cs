using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

namespace RandomlyGeneratedAisStream.Service;

public class RandomlyGeneratedAiStreamService : IService
{
	public event EventHandler<Ship>? ShipDataReceived;
	public event EventHandler<Position>? PositionDataReceived;

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
			ShipDataReceived?.Invoke(this, ship);
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
				PositionDataReceived?.Invoke(this, position);
			}

			await Task.Delay(TimeSpan.FromMilliseconds(500), token);
		}
	}
}
