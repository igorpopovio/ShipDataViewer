using Serilog;

using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using Stylet;

namespace ShipDataViewer.Shell;

public class ShellViewModel : Screen, IDisposable
{
	private readonly string DefaultLoadingMessage = "Ready";

	private CancellationTokenSource _cancellationTokenSource = new();
	private IService? _service;
	private readonly IWindowManager _windowManager;
	private readonly Func<ServiceConfiguration, IService> _serviceFactory;
	private readonly Func<SettingsDialogViewModel> _settingsDialogViewModelFactory;

	public string? ApiKey { get; set; }

	public ObservableUniqueCollection<Ship> Ships { get; } = [];

	public string FilterText { get; set; } = string.Empty;

	public string LoadingMessage { get; set; }

	public DateTime? LastUpdateReceived { get; set; }

	public string LastUpdateReceivedString { get; set; } = string.Empty;

	public string LastUpdateMessage => $"Last Update Received: {LastUpdateReceived:G}";

	public string ShipsReportedMessage => $"Total Ships Reported: {Ships.Count}";

	public int ShipCount => Ships.Count;

	public ShellViewModel(
		IWindowManager windowManager,
		Func<ServiceConfiguration, IService> serviceFactory,
		Func<SettingsDialogViewModel> settingsDialogViewModelFactory)
	{
		DisplayName = "Ship Data Viewer";
		LoadingMessage = DefaultLoadingMessage;
		_windowManager = windowManager;
		_serviceFactory = serviceFactory;
		_settingsDialogViewModelFactory = settingsDialogViewModelFactory;
	}

	public async Task StartListeningAsync()
	{
		if (ApiKey == null)
		{
			var settingsDialogViewModel = _settingsDialogViewModelFactory();
			_windowManager.ShowDialog(settingsDialogViewModel);
			if (!settingsDialogViewModel.Saved)
			{
				return;
			}

			ApiKey = settingsDialogViewModel.ApiKey;
		}

		LoadingMessage = "Listening to AIS data...";

		_service = _serviceFactory(new ServiceConfiguration
		{
			ApiKey = ApiKey!,
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
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = new CancellationTokenSource();
			UnsubscribeFromEvents();
			Log.Information("Cancelled loading ship data.");
		}
		catch (Exception exception)
		{
			var message = $"{exception.Message}\n{exception.StackTrace}";
			_windowManager.ShowMessageBox(message, "Error");
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = new CancellationTokenSource();
			Log.Error("Encountered exception.", exception);
		}
	}

	public async Task StopListeningAsync()
	{
		UnsubscribeFromEvents();

		await _cancellationTokenSource.CancelAsync();
		LoadingMessage = "Stopped listening to AIS data...";
	}

	public void OpenSettings()
	{
		var settingsDialogViewModel = _settingsDialogViewModelFactory();
		settingsDialogViewModel.ApiKey = ApiKey!;
		_windowManager.ShowDialog(settingsDialogViewModel);
		if (!settingsDialogViewModel.Saved)
		{
			return;
		}

		ApiKey = settingsDialogViewModel.ApiKey;
	}

	public bool Filter(object obj)
	{
		if (obj is not Ship ship)
		{
			return false;
		}

		return
			ship.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
			|| ship.Mmsi.ToString().Contains(FilterText)
			|| ship.ImoNumber.ToString().Contains(FilterText)
			|| ship.CallSign.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
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
		NotifyOfPropertyChange(() => ShipCount);
		LastUpdateReceived = DateTime.Now;
		LastUpdateReceivedString = LastUpdateReceived.Value.ToString("HH:mm:ss");
		Log.Debug($"Loaded '{ship.Name}'.");
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
		Log.Debug($"Loaded '{ship.Name} position data'.");
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
