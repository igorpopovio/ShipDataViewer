using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

using ShipDataViewer.Core;
using ShipDataViewer.Dtos;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShipDataViewer.Services;

public class AisStreamService : IService
{
	private const string UriString = "wss://stream.aisstream.io/v0/stream";
	private readonly ConnectionDetailsDto _connectionDetails;

	private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

	public AisStreamService(ConnectionDetailsDto connectionDetails)
	{
		_connectionDetails = connectionDetails;

		_jsonOptions.Converters.Add(new JsonStringEnumConverter());
		_jsonOptions.Converters.Add(new DateTimeJsonConverter());
		_jsonOptions.Converters.Add(new DateTimeNullableJsonConverter());
		_jsonOptions.Converters.Add(new DateOnlyJsonConverter());
		_jsonOptions.Converters.Add(new DateOnlyNullableJsonConverter());
		_jsonOptions.Converters.Add(new AddressedBinaryMessageJsonConverter());
		_jsonOptions.Converters.Add(new AddressedBinaryMessageApplicationIDJsonConverter());
		_jsonOptions.Converters.Add(new AddressedSafetyMessageJsonConverter());
		_jsonOptions.Converters.Add(new AidsToNavigationReportJsonConverter());
		_jsonOptions.Converters.Add(new AisMessageTypesJsonConverter());
		_jsonOptions.Converters.Add(new AisMessageTypesNullableJsonConverter());
		_jsonOptions.Converters.Add(new AisStreamMessageJsonConverter());
		_jsonOptions.Converters.Add(new AisStreamMessageMessageJsonConverter());
		_jsonOptions.Converters.Add(new AssignedModeCommandJsonConverter());
		_jsonOptions.Converters.Add(new AssignedModeCommandCommandsJsonConverter());
		_jsonOptions.Converters.Add(new AssignedModeCommandCommands0JsonConverter());
		_jsonOptions.Converters.Add(new BaseStationReportJsonConverter());
		_jsonOptions.Converters.Add(new BinaryAcknowledgeJsonConverter());
		_jsonOptions.Converters.Add(new BinaryAcknowledgeDestinationsJsonConverter());
		_jsonOptions.Converters.Add(new BinaryAcknowledgeDestinations0JsonConverter());
		_jsonOptions.Converters.Add(new BinaryBroadcastMessageJsonConverter());
		_jsonOptions.Converters.Add(new ChannelManagementJsonConverter());
		_jsonOptions.Converters.Add(new ChannelManagementAreaJsonConverter());
		_jsonOptions.Converters.Add(new ChannelManagementUnicastJsonConverter());
		_jsonOptions.Converters.Add(new CoordinatedUTCInquiryJsonConverter());
		_jsonOptions.Converters.Add(new DataLinkManagementMessageJsonConverter());
		_jsonOptions.Converters.Add(new DataLinkManagementMessageDataJsonConverter());
		_jsonOptions.Converters.Add(new DataLinkManagementMessageData0JsonConverter());
		_jsonOptions.Converters.Add(new ErrorJsonConverter());
		_jsonOptions.Converters.Add(new ExtendedClassBPositionReportJsonConverter());
		_jsonOptions.Converters.Add(new GnssBroadcastBinaryMessageJsonConverter());
		_jsonOptions.Converters.Add(new GroupAssignmentCommandJsonConverter());
		_jsonOptions.Converters.Add(new InterrogationJsonConverter());
		_jsonOptions.Converters.Add(new InterrogationStation1Msg1JsonConverter());
		_jsonOptions.Converters.Add(new InterrogationStation1Msg2JsonConverter());
		_jsonOptions.Converters.Add(new InterrogationStation2JsonConverter());
		_jsonOptions.Converters.Add(new LongRangeAisBroadcastMessageJsonConverter());
		_jsonOptions.Converters.Add(new MultiSlotBinaryMessageJsonConverter());
		_jsonOptions.Converters.Add(new PositionReportJsonConverter());
		_jsonOptions.Converters.Add(new SafetyBroadcastMessageJsonConverter());
		_jsonOptions.Converters.Add(new ShipStaticDataJsonConverter());
		_jsonOptions.Converters.Add(new ShipStaticDataDimensionJsonConverter());
		_jsonOptions.Converters.Add(new ShipStaticDataEtaJsonConverter());
		_jsonOptions.Converters.Add(new SingleSlotBinaryMessageJsonConverter());
		_jsonOptions.Converters.Add(new StandardClassBPositionReportJsonConverter());
		_jsonOptions.Converters.Add(new StandardSearchAndRescueAircraftReportJsonConverter());
		_jsonOptions.Converters.Add(new StaticDataReportJsonConverter());
		_jsonOptions.Converters.Add(new StaticDataReportReportAJsonConverter());
		_jsonOptions.Converters.Add(new StaticDataReportReportBJsonConverter());
		_jsonOptions.Converters.Add(new SubscriptionMessageJsonConverter());
		_jsonOptions.Converters.Add(new UnknownMessageJsonConverter());
	}

	public async Task Listen(CancellationToken token = default)
	{
		var connectionDetailsJson = JsonSerializer.Serialize(_connectionDetails);

		using var webSocket = new ClientWebSocket();
		await webSocket.ConnectAsync(new Uri(UriString), token);
		await webSocket.SendAsync(new ArraySegment<byte>(Encoding.Default.GetBytes(connectionDetailsJson)), WebSocketMessageType.Text, true, token);
		var buffer = new byte[4096];

		while (webSocket.State == WebSocketState.Open)
		{
			var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

			if (result.MessageType == WebSocketMessageType.Close)
			{
				await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
				return;
			}

			var responseJson = Encoding.Default.GetString(buffer, 0, result.Count);
			var obj = JsonSerializer.Deserialize<AisStreamMessage>(responseJson, _jsonOptions);
			var metadataJson = obj?.MetaData.ToString() ?? string.Empty;

			var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			jsonOptions.Converters.Add(new MetadataDateTimeConverter());
			var metadata = JsonSerializer.Deserialize<MetadataDto>(metadataJson, jsonOptions);
		}
	}
}
