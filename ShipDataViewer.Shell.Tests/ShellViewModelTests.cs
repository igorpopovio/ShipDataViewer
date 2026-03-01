using Autofac.Extras.Moq;

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
	public async Task CanListenToShipData()
	{
		await _shellViewModel.StartListeningAsync();

		_mock.Mock<IService>().Raise(service => service.ShipDataReceived += null, this, new Ship { Name = "Test Ship" });

		Assert.That(_shellViewModel.Ships, Has.Exactly(1).Matches<Ship>(s => s.Name == "Test Ship"));
	}
}
