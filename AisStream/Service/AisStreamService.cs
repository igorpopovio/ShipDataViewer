using Org.OpenAPITools.Model;

using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AisStream.Service;

public class AisStreamService : IService
{
	private readonly Uri _serviceUri = new("wss://stream.aisstream.io/v0/stream");

	private readonly ServiceConfiguration _serviceConfiguration;

	public event EventHandler<Ship>? ShipDataReceived;

	public event EventHandler<Position>? PositionDataReceived;

	public AisStreamService(ServiceConfiguration serviceConfiguration) => _serviceConfiguration = serviceConfiguration;

	public async Task ListenAsync(CancellationToken cancellationToken = default)
	{
		using var webSocket = new ClientWebSocket();
		await webSocket.ConnectAsync(_serviceUri, cancellationToken);

		var serviceConfigurationJson = JsonSerializer.Serialize(_serviceConfiguration);
		var serviceConfigurationBytes = Encoding.UTF8.GetBytes(serviceConfigurationJson);
		await webSocket.SendAsync(new ArraySegment<byte>(serviceConfigurationBytes), WebSocketMessageType.Text, true, cancellationToken);

		var buffer = new byte[4096];
		var jsonSerializerOptions = CreateJsonOptions();
		while (webSocket.State == WebSocketState.Open)
		{
			var response = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

			if (response.MessageType == WebSocketMessageType.Close)
			{
				await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
				return;
			}

			var responseJson = Encoding.UTF8.GetString(buffer, 0, response.Count);
			var aisStreamMessage = JsonSerializer.Deserialize<AisStreamMessage>(responseJson, jsonSerializerOptions);

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
				case AisMessageTypes.StaticDataReport:
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
			CallSign = shipData.CallSign,
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
