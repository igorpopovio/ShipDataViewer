using ShipDataViewer.Dtos;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ShipDataViewer.Services;

public class AisStreamService : IService
{
	private const string UriString = "wss://stream.aisstream.io/v0/stream";
	private readonly ConnectionDetailsDto _connectionDetails;

	public AisStreamService(ConnectionDetailsDto connectionDetails)
	{
		_connectionDetails = connectionDetails;
	}

	public async Task Listen(CancellationToken token = default)
	{
		var connectionDetailsJson = JsonSerializer.Serialize(_connectionDetails);

		using var webSocket = new ClientWebSocket();
		await webSocket.ConnectAsync(new Uri(UriString), token);
		await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(connectionDetailsJson)), WebSocketMessageType.Text, true, token);
		var buffer = new byte[4096];

		while (webSocket.State == WebSocketState.Open)
		{
			var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

			if (result.MessageType == WebSocketMessageType.Close)
			{
				await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
				return;
			}

			var x = $"Received {Encoding.Default.GetString(buffer, 0, result.Count)}";
		}
	}
}
