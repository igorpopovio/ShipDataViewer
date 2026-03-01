using Autofac.Extras.Moq;

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
	public async Task CanListenToChanges()
	{
		using var mock = AutoMock.GetLoose();
		var shellViewModel = mock.Create<ShellViewModel>();
		shellViewModel.ApiKey = "dummy-key-for-tests";
		await shellViewModel.StartListeningAsync();

		Assert.That(shellViewModel.DisplayName, Is.EqualTo("Ship Data Viewer"));
	}
}
