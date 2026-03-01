using Autofac.Extras.Moq;

using ShipDataViewer.Core.Model;
using ShipDataViewer.Core.Service;

namespace ShipDataViewer.Shell.Tests;

public class ShellViewModelTests
{
	[Test]
	public void HasDisplayName()
	{
		using var mock = AutoMock.GetLoose();
		var shellViewModel = mock.Create<ShellViewModel>();

		Assert.That(shellViewModel.DisplayName, Is.EqualTo("Ship Data Viewer"));
	}

	[Test]
	public async Task UpdatesLoadingMessage()
	{
		using var mock = AutoMock.GetLoose();
		var shellViewModel = mock.Create<ShellViewModel>();
		shellViewModel.ApiKey = "dummy-key-for-tests";

		Assert.That(shellViewModel.LoadingMessage, Is.EqualTo("Ready"));

		await shellViewModel.StartListeningAsync();
		Assert.That(shellViewModel.LoadingMessage, Is.EqualTo("Listening to AIS data..."));

		await shellViewModel.StopListeningAsync();
		Assert.That(shellViewModel.LoadingMessage, Is.EqualTo("Stopped listening to AIS data..."));
	}

	[Test]
	public async Task CanListenToShipData()
	{
		using var mock = AutoMock.GetLoose();
		var shellViewModel = mock.Create<ShellViewModel>();
		shellViewModel.ApiKey = "dummy-key-for-tests";

		await shellViewModel.StartListeningAsync();

		mock.Mock<IService>().Raise(service => service.ShipDataReceived += null, this, new Ship { Name = "Test Ship" });

		Assert.That(shellViewModel.Ships, Has.Exactly(1).Matches<Ship>(s => s.Name == "Test Ship"));
	}
}
