using Stylet;

using System.Net.WebSockets;
using System.Text;

namespace ShipDataViewer;

public class ShellViewModel : Screen
{
	private readonly string DefaultLoadingMessage = "Ready";

	private CancellationTokenSource cts = new CancellationTokenSource();

	private readonly IWindowManager _windowManager;

	public string Details { get; set; } = string.Empty;
	public string LoadingMessage { get; set; }

	public ShellViewModel(IWindowManager windowManager)
	{
		DisplayName = "Ship Data Viewer";
		LoadingMessage = DefaultLoadingMessage;
		_windowManager = windowManager;
	}

	public async Task LoadShipDetails()
	{
		LoadingMessage = "Connecting to server...";
		await LoadShipDetailsInternal();
		Details = "Data loaded";
		LoadingMessage = "Data loaded successfully!";
		await Task.Delay(2000);
		LoadingMessage = DefaultLoadingMessage;
	}

	public async Task CancelSubscription()
	{
		await cts.CancelAsync();
	}

	public async Task LoadShipDetailsInternal()
	{
		var apiKey = Environment.GetEnvironmentVariable("AIS_STREAM_API_KEY");
		var text = "{ \"APIKey\": \"" + apiKey + "\", \"BoundingBoxes\": [[[-11.0, 178.0], [30.0, 74.0]]]}";

		using var ws = new ClientWebSocket();
		await ws.ConnectAsync(new Uri("wss://stream.aisstream.io/v0/stream"), cts.Token);
		await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)), WebSocketMessageType.Text, true, cts.Token);
		byte[] buffer = new byte[4096];
		try
		{
			while (ws.State == WebSocketState.Open)
			{
				var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
				if (result.MessageType == WebSocketMessageType.Close)
				{
					await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cts.Token);
				}
				else
				{
					Details = $"Received {Encoding.Default.GetString(buffer, 0, result.Count)}";
				}
			}
		}
		catch (OperationCanceledException)
		{
			Details = "Subscription cancelled.";
			cts.Dispose();
			cts = new CancellationTokenSource();
			return;
		}
	}
}
