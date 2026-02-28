using Autofac;

using Stylet;

namespace ShipDataViewer.Ui;

/// <summary>
/// Code originally from:
/// https://github.com/canton7/Stylet/blob/master/Bootstrappers/AutofacBootstrapper.cs
/// </summary>
public class AutofacBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
{
	private IContainer? _container;

	private TRootViewModel? _rootViewModel;
	protected virtual TRootViewModel? RootViewModel => _rootViewModel ??= GetInstance(typeof(TRootViewModel)) as TRootViewModel;

	protected override void ConfigureBootstrapper()
	{
		var builder = new ContainerBuilder();
		DefaultConfigureIoC(builder);
		ConfigureIoC(builder);
		_container = builder.Build();
	}

	protected virtual void DefaultConfigureIoC(ContainerBuilder builder)
	{
		var viewManagerConfig = new ViewManagerConfig()
		{
			ViewFactory = GetInstance,
			ViewAssemblies = [GetType().Assembly]
		};
		builder.RegisterInstance<IViewManager>(new ViewManager(viewManagerConfig));
		builder.RegisterType<MessageBoxView>();

		builder.RegisterInstance<IWindowManagerConfig>(this).ExternallyOwned();
		builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
		builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
		builder.RegisterType<MessageBoxViewModel>().As<IMessageBoxViewModel>().ExternallyOwned(); // Not singleton!

		// See https://github.com/canton7/Stylet/discussions/211
		builder.RegisterAssemblyTypes(GetType().Assembly).Where(x => !x.Name.Contains("ProcessedByFody")).ExternallyOwned();
	}

	protected virtual void ConfigureIoC(ContainerBuilder builder) { }

	public override object? GetInstance(Type type)
	{
		return _container?.Resolve(type);
	}

	protected override void Launch()
	{
		base.DisplayRootView(RootViewModel);
	}

	public override void Dispose()
	{
		ScreenExtensions.TryDispose(_rootViewModel);
		_container?.Dispose();
		base.Dispose();
	}
}
