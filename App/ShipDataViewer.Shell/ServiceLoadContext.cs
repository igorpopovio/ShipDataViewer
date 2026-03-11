using ShipDataViewer.Core.Service;

using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace ShipDataViewer.Shell;

public class ServiceLoadContext : AssemblyLoadContext
{
	private readonly AssemblyDependencyResolver _resolver;

	public string ServiceName { get; private set; }
	public IService? Service { get; set; }

	public ServiceLoadContext(string servicePath) : base(isCollectible: true)
	{
		_resolver = new AssemblyDependencyResolver(servicePath);
		ServiceName = Path.GetFileName(servicePath);
	}

	protected override Assembly? Load(AssemblyName assemblyName)
	{
		var path = _resolver.ResolveAssemblyToPath(assemblyName);
		return path != null ? LoadFromAssemblyPath(path) : null;
	}
}
