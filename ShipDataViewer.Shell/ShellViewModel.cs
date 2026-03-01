using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using Stylet;

namespace ShipDataViewer.Shell;

public class ShellViewModel : Screen, IDisposable
{
	private readonly string DefaultLoadingMessage = "Ready";

	private CancellationTokenSource _cancellationTokenSource = new();
	private IService? _service;
	private readonly Func<ServiceConfiguration, IService> _serviceFactory;

	public string? ApiKey { get; set; }

	public ObservableUniqueCollection<Ship> Ships { get; } = [];

	public string FilterText { get; set; } = string.Empty;

	public string LoadingMessage { get; set; }

	public DateTime? LastUpdateReceived { get; set; }
	public string LastUpdateMessage => $"Last Update Received: {LastUpdateReceived:G}";
	public string ShipsReportedMessage => $"Total Ships Reported: {Ships.Count}";

	public ShellViewModel(Func<ServiceConfiguration, IService> serviceFactory)
	{
		DisplayName = "Ship Data Viewer";
		LoadingMessage = DefaultLoadingMessage;
		_serviceFactory = serviceFactory;
	}

	public async Task StartListeningAsync()
	{
		LoadingMessage = "Listening to AIS data...";
		var apiKey = ApiKey ?? Environment.GetEnvironmentVariable("AIS_STREAM_API_KEY") ?? throw new ArgumentNullException("AIS_STREAM_API_KEY");
		_service = _serviceFactory(new ServiceConfiguration
		{
			ApiKey = apiKey,
			BoundingBoxes = [[[-11, 178], [30, 74]]],
		});

		_service.ShipDataReceived += OnShipDataReceived;
		_service.PositionDataReceived += OnShipPositionDataReceived;

		try
		{
			await _service.ListenAsync(_cancellationTokenSource.Token);
		}
		catch (OperationCanceledException)
		{
			UnsubscribeFromEvents();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = new CancellationTokenSource();
		}
	}

	public async Task StopListeningAsync()
	{
		UnsubscribeFromEvents();

		await _cancellationTokenSource.CancelAsync();
		LoadingMessage = "Stopped listening to AIS data...";
	}

	public bool Filter(object obj)
	{
		if (obj is not Ship ship)
		{
			return false;
		}

		return
			ship.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
			|| ship.Mmsi.ToString().Contains(FilterText);
	}

	public void Dispose()
	{
		UnsubscribeFromEvents();
		_cancellationTokenSource.Dispose();
	}

	private void OnShipDataReceived(object? sender, Ship ship)
	{
		Ships.Add(ship);
		NotifyOfPropertyChange(() => ShipsReportedMessage);
		LastUpdateReceived = DateTime.Now;
	}

	private void OnShipPositionDataReceived(object? sender, Position position)
	{
		var ship = Ships.SingleOrDefault(s => s.Mmsi == position.ShipMmsi);
		if (ship is null)
		{
			return;
		}

		ship.LastReportedPosition = position;
		ship.LastUpdated = DateTime.Now;
		LastUpdateReceived = ship.LastUpdated;
	}

	private void UnsubscribeFromEvents()
	{
		if (_service == null)
		{
			return;
		}

		_service.ShipDataReceived -= OnShipDataReceived;
		_service.PositionDataReceived -= OnShipPositionDataReceived;
		_service = null;
	}
}
