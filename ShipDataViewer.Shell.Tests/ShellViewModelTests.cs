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
}
