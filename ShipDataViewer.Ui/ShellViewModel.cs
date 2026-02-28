using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using Stylet;

using System.Collections.ObjectModel;

namespace ShipDataViewer.Ui;

public class ShellViewModel : Screen
{
	private readonly string DefaultLoadingMessage = "Ready";

	private CancellationTokenSource cts = new CancellationTokenSource();

	private readonly IWindowManager _windowManager;
	private readonly Func<ServiceConfiguration, IService> _serviceFactory;

	public ObservableCollection<Ship> Ships { get; } = new ObservableCollection<Ship>();

	public string LoadingMessage { get; set; }

	public ShellViewModel(IWindowManager windowManager, Func<ServiceConfiguration, IService> serviceFactory)
	{
		DisplayName = "Ship Data Viewer";
		LoadingMessage = DefaultLoadingMessage;
		_windowManager = windowManager;
		_serviceFactory = serviceFactory;
	}

	public async Task StartListening()
	{
		LoadingMessage = "Listening to AIS data...";
		var apiKey = Environment.GetEnvironmentVariable("AIS_STREAM_API_KEY");
		var service = _serviceFactory(new ServiceConfiguration
		{
			ApiKey = apiKey!,
			BoundingBoxes = [[[-11, 178], [30, 74]]],
		});

		service.ShipDataReceived += (sender, ship) => { Ships.Add(ship); };

		try
		{
			await service.ListenAsync(cts.Token);
		}
		catch (OperationCanceledException)
		{
			cts.Dispose();
			cts = new CancellationTokenSource();
			LoadingMessage = "Stopped listening to AIS data...";
		}
	}

	public async Task StopListening()
	{
		await cts.CancelAsync();
	}
}
