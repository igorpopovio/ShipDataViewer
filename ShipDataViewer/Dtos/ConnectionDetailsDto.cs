namespace ShipDataViewer.Dtos;

public class ConnectionDetailsDto
{
	public string ApiKey { get; set; } = string.Empty;

	public long[][][] BoundingBoxes { get; set; } = [];
}
