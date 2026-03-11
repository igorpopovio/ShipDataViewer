using ShipDataViewer.Core.Service;

using System.IO;

namespace ShipDataViewer.Shell;

// https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
// https://github.com/dotnet/samples/tree/main/core/extensions/AppWithPlugin
public class ServiceLoader
{
	private const string DirectoryName = "services";

	public Lazy<List<ServiceLoadContext>> ServiceContexts = new Lazy<List<ServiceLoadContext>>(LoadServices);

	private static List<ServiceLoadContext> LoadServices()
	{
		var contexts = new List<ServiceLoadContext>();

		if (!Directory.Exists(DirectoryName))
		{
			return contexts;
		}

		foreach (var dll in Directory.GetFiles(DirectoryName, "*.dll"))
		{
			var fullPath = Path.GetFullPath(dll);
			var context = new ServiceLoadContext(fullPath);
			var assembly = context.LoadFromAssemblyPath(fullPath);

			var serviceTypes = assembly
				.GetTypes()
				.Where(t => typeof(IService).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

			foreach (var type in serviceTypes)
			{
				var service = (IService)Activator.CreateInstance(type)!;
				context.Service = service;
				contexts.Add(context);
			}
		}

		return contexts;
	}
}
