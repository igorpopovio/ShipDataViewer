using Serilog;

using System.Windows;

namespace ShipDataViewer;

public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		Log.Logger = new LoggerConfiguration()
			.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
			.CreateLogger();
		Log.Information("Application has started!");
	}
}
