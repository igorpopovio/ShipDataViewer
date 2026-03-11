using Org.OpenAPITools.Model;

using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AisWebSocketStream.Service;

// https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/libraries#websocket-enhancements
public class AisWebSocketStreamService : IService
{
	private readonly Uri _serviceUri = new("wss://stream.aisstream.io/v0/stream");

	private readonly ServiceConfiguration _serviceConfiguration;

	public event EventHandler<Ship>? ShipDataReceived;

	public event EventHandler<Position>? PositionDataReceived;

	public AisWebSocketStreamService(ServiceConfiguration serviceConfiguration) => _serviceConfiguration = serviceConfiguration;

	public async Task ListenAsync(CancellationToken cancellationToken = default)
	{
		var webSocket = new ClientWebSocket();
		await webSocket.ConnectAsync(_serviceUri, cancellationToken);

		var stream = WebSocketStream.Create(webSocket, WebSocketMessageType.Text, ownsWebSocket: false);

		var serviceConfigurationJson = JsonSerializer.Serialize(_serviceConfiguration);
		var writer = new StreamWriter(stream, leaveOpen: true);
		await writer.WriteLineAsync(serviceConfigurationJson);
		await writer.DisposeAsync();

		var jsonSerializerOptions = CreateJsonOptions();

		var reader = new StreamReader(stream);
		var buffer = new char[4096];
		while (!cancellationToken.IsCancellationRequested)
		{
			var count = await reader.ReadAsync(buffer, 0, buffer.Length);
			var message = new string(buffer, 0, count);

			var aisStreamMessage = JsonSerializer.Deserialize<AisStreamMessage>(message, jsonSerializerOptions);
			if (aisStreamMessage == null)
			{
				continue;
			}

			switch (aisStreamMessage.MessageType)
			{
				case AisMessageTypes.ShipStaticData:
					Process(aisStreamMessage.Message.ShipStaticData);
					break;
				case AisMessageTypes.PositionReport:
					Process(aisStreamMessage.Message.PositionReport);
					break;
				default:
					break;
			}
		}
	}

	private void Process(PositionReport? positionReport)
	{
		if (positionReport == null)
		{
			return;
		}

		var position = new Position
		{
			ShipMmsi = positionReport.UserID,
			Latitude = positionReport.Latitude,
			Longitude = positionReport.Longitude,
			Sog = positionReport.Sog,
			Cog = positionReport.Cog,
			TrueHeading = positionReport.TrueHeading,
		};

		PositionDataReceived?.Invoke(this, position);
	}

	private void Process(ShipStaticData? shipData)
	{
		if (shipData == null || shipData.ImoNumber == 0)
		{
			return;
		}

		var ship = new Ship
		{
			Mmsi = shipData.UserID,
			Name = shipData.Name.Trim(),
			CallSign = shipData.CallSign.Trim(),
			ImoNumber = shipData.ImoNumber,
		};

		ShipDataReceived?.Invoke(this, ship);
	}

	private JsonSerializerOptions CreateJsonOptions()
	{
		var assembly = typeof(AisStreamMessage).Assembly;
		var converterTypes = assembly
			.GetTypes()
			.Where(t =>
				!t.IsAbstract &&
				!t.IsInterface &&
				typeof(JsonConverter).IsAssignableFrom(t))
			.ToList();

		var options = new JsonSerializerOptions();
		foreach (var type in converterTypes)
		{
			if (Activator.CreateInstance(type) is JsonConverter converter)
			{
				options.Converters.Add(converter);
			}
		}

		return options;
	}
}
