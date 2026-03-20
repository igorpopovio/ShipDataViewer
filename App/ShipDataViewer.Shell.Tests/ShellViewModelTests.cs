using Autofac.Extras.Moq;

using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

using System.Threading.Channels;

namespace ShipDataViewer.Shell.Tests;

public class ShellViewModelTests
{
	private AutoMock _mock;
	private ShellViewModel _shellViewModel;
	private Channel<Ship> _shipChannel;
	private Channel<Position> _positionChannel;

	[SetUp]
	public void Setup()
	{
		_mock = AutoMock.GetLoose();
		_shellViewModel = _mock.Create<ShellViewModel>();
		_shellViewModel.ApiKey = "dummy-key-for-tests";

		_shipChannel = Channel.CreateUnbounded<Ship>();
		_mock.Mock<IService>().Setup(service => service.ShipData).Returns(_shipChannel.Reader);

		_positionChannel = Channel.CreateUnbounded<Position>();
		_mock.Mock<IService>().Setup(service => service.PositionData).Returns(_positionChannel.Reader);
	}

	[TearDown]
	public void Teardown()
	{
		_mock.Dispose();
		_shellViewModel.Dispose();
	}

	[Test]
	public void HasDisplayName()
	{
		Assert.That(_shellViewModel.DisplayName, Is.EqualTo("Ship Data Viewer"));
	}

	[Test]
	public async Task UpdatesLoadingMessage()
	{
		Assert.That(_shellViewModel.LoadingMessage, Is.EqualTo("Ready"));

		var startListeningTask = _shellViewModel.StartListeningAsync();

		Assert.That(_shellViewModel.LoadingMessage, Is.EqualTo("Listening to AIS data..."));

		var stopListeningTask = _shellViewModel.StopListeningAsync();

		await Task.WhenAll(startListeningTask, stopListeningTask);

		Assert.That(_shellViewModel.LoadingMessage, Is.EqualTo("Stopped listening to AIS data..."));
	}

	[Test]
	public async Task UpdatesShipData()
	{
		var startListeningTask = _shellViewModel.StartListeningAsync();
		var writeShipTask = _shipChannel.Writer.WriteAsync(new Ship { Name = "Test Ship" });
		var stopListeningTask = _shellViewModel.StopListeningAsync();

		await Task.WhenAll(startListeningTask, writeShipTask.AsTask(), stopListeningTask);

		Assert.That(_shellViewModel.Ships, Has.Exactly(1).Matches<Ship>(s => s.Name == "Test Ship"));
	}

	[Test]
	public async Task ShipDataIsUnique()
	{
		var startListeningTask = _shellViewModel.StartListeningAsync();

		var ship = new Ship { Mmsi = 1, Name = "Test Ship" };
		var writeShipTask = _shipChannel.Writer.WriteAsync(ship);
		var writeSameShipAgainTask = _shipChannel.Writer.WriteAsync(ship);

		var stopListeningTask = _shellViewModel.StopListeningAsync();

		await Task.WhenAll(startListeningTask, writeShipTask.AsTask(), writeSameShipAgainTask.AsTask(), stopListeningTask);

		Assert.That(_shellViewModel.Ships, Has.Exactly(1).Matches<Ship>(s => s.Name == "Test Ship"));
	}

	[Test]
	public async Task UpdatesShipPosition()
	{
		var startListeningTask = _shellViewModel.StartListeningAsync();

		var ship = new Ship { Mmsi = 1, Name = "Test Ship" };
		var writeShipTask = _shipChannel.Writer.WriteAsync(ship);

		Assert.That(ship.LastReportedPosition, Is.Null);

		var position = new Position { ShipMmsi = ship.Mmsi, Cog = 10 };
		var writePositionTask = _positionChannel.Writer.WriteAsync(position);

		var stopListeningTask = _shellViewModel.StopListeningAsync();

		await Task.WhenAll(startListeningTask, writeShipTask.AsTask(), writePositionTask.AsTask(), stopListeningTask);

		Assert.That(ship.LastReportedPosition, Is.EqualTo(position));
		Assert.That(ship.LastUpdated, Is.Not.Null);
	}

	[Test]
	public async Task StopsListeningToUpdatesOnceCancelled()
	{
		var startListeningTask = _shellViewModel.StartListeningAsync();
		var stopListeningTask = _shellViewModel.StopListeningAsync();
		await Task.WhenAll(startListeningTask, stopListeningTask);

		var ship = new Ship { Mmsi = 1, Name = "Test Ship" };
		await _shipChannel.Writer.WriteAsync(ship);

		Assert.That(_shellViewModel.Ships, Has.Count.EqualTo(0));
	}

	[Test]
	public async Task CanFilter()
	{
		using (Assert.EnterMultipleScope())
		{
			_shellViewModel.FilterText = "ship";
			Assert.That(_shellViewModel.Filter(new Ship { Mmsi = 1234, Name = "Ship1" }), "Matches filter");

			_shellViewModel.FilterText = "some filter that will not find anything";
			Assert.That(_shellViewModel.Filter(new Ship { Mmsi = 1234, Name = "Ship1" }), Is.False, "Matches filter");

			_shellViewModel.FilterText = "123";
			Assert.That(_shellViewModel.Filter(new Ship { Mmsi = 1234, Name = "Ship1" }), "Matches Mmsi partially");

			_shellViewModel.FilterText = "123";
			Assert.That(_shellViewModel.Filter(new Ship { ImoNumber = 1234 }), "Matches ImoNumber partially");

			_shellViewModel.FilterText = "some";
			Assert.That(_shellViewModel.Filter(new Ship { CallSign = "something" }), "Matches CallSign partially");
		}
	}
}
