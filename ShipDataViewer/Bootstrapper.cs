using Stylet;

using StyletIoC;

namespace ShipDataViewer;

public class Bootstrapper : Bootstrapper<ShellViewModel>
{
	protected override void ConfigureIoC(IStyletIoCBuilder builder)
	{
		base.ConfigureIoC(builder);

		//builder.Bind<IDialogFactory>().ToAbstractFactory();
	}
}
