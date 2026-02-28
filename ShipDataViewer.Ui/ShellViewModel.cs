using ShipDataViewer.Core;
using ShipDataViewer.Dtos;

using Stylet;

namespace ShipDataViewer;

public class ShellViewModel : Screen
{
	private readonly string DefaultLoadingMessage = "Ready";

	private CancellationTokenSource cts = new CancellationTokenSource();

	private readonly IWindowManager _windowManager;
	private readonly Func<ConnectionDetailsDto, IService> _serviceFactory;

	public string Details { get; set; } = string.Empty;
	public string LoadingMessage { get; set; }

	public ShellViewModel(IWindowManager windowManager, Func<ConnectionDetailsDto, IService> serviceFactory)
	{
		DisplayName = "Ship Data Viewer";
		LoadingMessage = DefaultLoadingMessage;
		_windowManager = windowManager;
		_serviceFactory = serviceFactory;
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
		var service = _serviceFactory(new ConnectionDetailsDto
		{
			ApiKey = apiKey!,
			BoundingBoxes = [[[-11, 178], [30, 74]]],
		});

		try
		{
			await service.Listen(cts.Token);
			Details = "works";
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
