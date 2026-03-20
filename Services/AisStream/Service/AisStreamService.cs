using Org.OpenAPITools.Model;

using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace AisStream.Service;

public class AisStreamService : IService
{
	private readonly Uri _serviceUri = new("wss://stream.aisstream.io/v0/stream");

	private readonly ServiceConfiguration _serviceConfiguration;
	private readonly Channel<Ship> _shipData;
	private readonly Channel<Position> _positionData;

	public ChannelReader<Ship> ShipData => _shipData.Reader;

	public ChannelReader<Position> PositionData => _positionData.Reader;

	public AisStreamService(ServiceConfiguration serviceConfiguration)
	{
		_serviceConfiguration = serviceConfiguration;
		_shipData = Channel.CreateUnbounded<Ship>();
		_positionData = Channel.CreateUnbounded<Position>();
	}

	public async Task ListenAsync(CancellationToken cancellationToken = default)
	{
		using var webSocket = new ClientWebSocket();
		await webSocket.ConnectAsync(_serviceUri, cancellationToken).ConfigureAwait(false);

		var serviceConfigurationJson = JsonSerializer.Serialize(_serviceConfiguration);
		var serviceConfigurationBytes = Encoding.UTF8.GetBytes(serviceConfigurationJson);
		await webSocket.SendAsync(new ArraySegment<byte>(serviceConfigurationBytes), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);

		var buffer = new byte[4096];
		var jsonSerializerOptions = CreateJsonOptions();
		while (webSocket.State == WebSocketState.Open)
		{
			var response = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

			if (response.MessageType == WebSocketMessageType.Close)
			{
				await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken).ConfigureAwait(false);
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
					await ProcessAsync(aisStreamMessage.Message.ShipStaticData, cancellationToken).ConfigureAwait(false);
					break;
				case AisMessageTypes.PositionReport:
					await ProcessAsync(aisStreamMessage.Message.PositionReport, cancellationToken).ConfigureAwait(false);
					break;
				default:
					break;
			}
		}
	}

	private async Task ProcessAsync(ShipStaticData? shipData, CancellationToken cancellationToken)
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

		await _shipData.Writer.WriteAsync(ship, cancellationToken).ConfigureAwait(false);
	}

	private async Task ProcessAsync(PositionReport? positionReport, CancellationToken cancellationToken)
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

		await _positionData.Writer.WriteAsync(position, cancellationToken).ConfigureAwait(false);
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
