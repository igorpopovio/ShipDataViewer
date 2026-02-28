using ShipDataViewer.Core;
using ShipDataViewer.Services;

using Stylet;

using StyletIoC;

namespace ShipDataViewer;

public class Bootstrapper : Bootstrapper<ShellViewModel>
{
	protected override void ConfigureIoC(IStyletIoCBuilder builder)
	{
		base.ConfigureIoC(builder);

		builder.Bind<IService>().To<AisStreamService>();
	}
}
