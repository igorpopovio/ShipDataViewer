using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using Stylet;

using System.Collections.ObjectModel;

namespace ShipDataViewer.Ui;

public class ShellViewModel : Screen
{
	private readonly string DefaultLoadingMessage = "Ready";

	private CancellationTokenSource cancellationTokenSource = new();

	private readonly Func<ServiceConfiguration, IService> _serviceFactory;

	public ObservableCollection<Ship> Ships { get; } = [];

	public string FilterText { get; set; } = string.Empty;

	public string LoadingMessage { get; set; }

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

		service.ShipDataReceived += (sender, ship) => { Ships.Add(ship); };

		try
		{
			await service.ListenAsync(cancellationTokenSource.Token);
		}
		catch (OperationCanceledException)
		{
			cancellationTokenSource.Dispose();
			cancellationTokenSource = new CancellationTokenSource();
			LoadingMessage = "Stopped listening to AIS data...";
		}
	}

	public async Task StopListening()
	{
		await cancellationTokenSource.CancelAsync();
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
