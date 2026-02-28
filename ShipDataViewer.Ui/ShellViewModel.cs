using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using Stylet;

namespace ShipDataViewer.Ui;

public class ShellViewModel : Screen
{
	private readonly string DefaultLoadingMessage = "Ready";

	private CancellationTokenSource _cancellationTokenSource = new();

	private readonly Func<ServiceConfiguration, IService> _serviceFactory;

	public ObservableUniqueCollection<Ship> Ships { get; } = [];

	public string FilterText { get; set; } = string.Empty;

	public string LoadingMessage { get; set; }

	public DateTime LastUpdateReceived { get; set; }
	public string LastUpdateMessage => $"Last Update Received: {LastUpdateReceived:G}";
	public string ShipsReportedMessage => $"Total Ships Reported: {Ships.Count}";

	public ShellViewModel(Func<ServiceConfiguration, IService> serviceFactory)
	{
		DisplayName = "Ship Data Viewer";
		LoadingMessage = DefaultLoadingMessage;
		_serviceFactory = serviceFactory;
	}

	public async Task StartListening()
	{
		LoadingMessage = "Listening to AIS data...";
		var apiKey = Environment.GetEnvironmentVariable("AIS_STREAM_API_KEY") ?? throw new ArgumentNullException("AIS_STREAM_API_KEY");
		var service = _serviceFactory(new ServiceConfiguration
		{
			ApiKey = apiKey,
			BoundingBoxes = [[[-11, 178], [30, 74]]],
		});

		service.ShipDataReceived += (sender, ship) =>
		{
			Ships.Add(ship);
			NotifyOfPropertyChange(() => ShipsReportedMessage);
			LastUpdateReceived = DateTime.Now;
		};

		try
		{
			await service.ListenAsync(_cancellationTokenSource.Token);
		}
		catch (OperationCanceledException)
		{
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = new CancellationTokenSource();
			LoadingMessage = "Stopped listening to AIS data...";
		}
	}

	public async Task StopListening()
	{
		await _cancellationTokenSource.CancelAsync();
	}

	public bool Filter(object obj)
	{
		if (obj is not Ship ship)
		{
			return false;
		}

		return ship.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
	}
}
