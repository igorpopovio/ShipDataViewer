namespace ShipDataViewer.Core.Service;

public class ServiceConfiguration
{
	public string ApiKey { get; set; } = string.Empty;

	public long[][][] BoundingBoxes { get; set; } = [];
}
