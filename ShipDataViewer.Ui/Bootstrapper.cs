using AisStream.Service;

using Autofac;

namespace ShipDataViewer.Ui;

public class Bootstrapper : AutofacBootstrapper<ShellViewModel>
{
	protected override void ConfigureIoC(ContainerBuilder builder)
	{
		base.ConfigureIoC(builder);
		builder.RegisterType<AisStreamService>().AsImplementedInterfaces();
		// builder.RegisterAssemblyTypes(typeof(AisStreamService).Assembly).AsImplementedInterfaces();
	}
}
