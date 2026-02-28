using Autofac;

using ShipDataViewer.Services;
using ShipDataViewer.Ui;

namespace ShipDataViewer;

public class Bootstrapper : AutofacBootstrapper<ShellViewModel>
{
	protected override void ConfigureIoC(ContainerBuilder builder)
	{
		base.ConfigureIoC(builder);
		builder.RegisterType<AisStreamService>().AsImplementedInterfaces();
		// builder.RegisterAssemblyTypes(typeof(AisStreamService).Assembly).AsImplementedInterfaces();
	}
}
