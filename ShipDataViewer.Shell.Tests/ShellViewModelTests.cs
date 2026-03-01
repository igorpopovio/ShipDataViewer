using Autofac.Extras.Moq;

using Moq;

using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

namespace ShipDataViewer.Shell.Tests;

public class ShellViewModelTests
{
	private AutoMock _mock;
	private ShellViewModel _shellViewModel;

	[SetUp]
	public void Setup()
	{
		_mock = AutoMock.GetLoose();
		_shellViewModel = _mock.Create<ShellViewModel>();
		_shellViewModel.ApiKey = "dummy-key-for-tests";
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

		await _shellViewModel.StartListeningAsync();
		Assert.That(_shellViewModel.LoadingMessage, Is.EqualTo("Listening to AIS data..."));

		await _shellViewModel.StopListeningAsync();
		Assert.That(_shellViewModel.LoadingMessage, Is.EqualTo("Stopped listening to AIS data..."));
	}

	[Test]
	public async Task UpdatesShipData()
	{
		await _shellViewModel.StartListeningAsync();

		_mock.Mock<IService>().Raise(service => service.ShipDataReceived += null, this, new Ship { Name = "Test Ship" });

		Assert.That(_shellViewModel.Ships, Has.Exactly(1).Matches<Ship>(s => s.Name == "Test Ship"));
	}

	[Test]
	public async Task ShipDataIsUnique()
	{
		await _shellViewModel.StartListeningAsync();

		var ship = new Ship { Mmsi = 1, Name = "Test Ship" };

		_mock.Mock<IService>().Raise(service => service.ShipDataReceived += null, this, ship);
		_mock.Mock<IService>().Raise(service => service.ShipDataReceived += null, this, ship);

		Assert.That(_shellViewModel.Ships, Has.Exactly(1).Matches<Ship>(s => s.Name == "Test Ship"));
	}

	[Test]
	public async Task UpdatesShipPosition()
	{
		await _shellViewModel.StartListeningAsync();

		var ship = new Ship { Mmsi = 1, Name = "Test Ship" };
		_mock.Mock<IService>().Raise(service => service.ShipDataReceived += null, this, ship);
		Assert.That(ship.LastReportedPosition, Is.Null);

		var position = new Position { ShipMmsi = ship.Mmsi, Cog = 10 };
		_mock.Mock<IService>().Raise(service => service.PositionDataReceived += null, this, position);

		Assert.That(ship.LastReportedPosition, Is.EqualTo(position));
		Assert.That(ship.LastUpdated, Is.Not.Null);
	}

	[Test]
	public async Task StopsListeningToUpdatesOnceCancelled()
	{
		_mock
			.Mock<IService>()
			.Setup(service => service.ListenAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new OperationCanceledException());

		await _shellViewModel.StartListeningAsync();

		_mock.Mock<IService>().Raise(service => service.ShipDataReceived += null, this, new Ship { Mmsi = 1, Name = "Test Ship" });

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
