using AisWebSocketStream.Service;

using Autofac;

namespace ShipDataViewer.Shell;

public class Bootstrapper : AutofacBootstrapper<ShellViewModel>
{
	protected override void ConfigureIoC(ContainerBuilder builder)
	{
		base.ConfigureIoC(builder);
		//builder.RegisterType<AisStreamService>().AsImplementedInterfaces();
		//builder.RegisterType<RandomlyGeneratedAiStreamService>().AsImplementedInterfaces();
		builder.RegisterType<AisWebSocketStreamService>().AsImplementedInterfaces();
	}
}
