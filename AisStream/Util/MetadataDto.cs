namespace AisStream.Utils;

public class MetadataDto
{
	public long MMSI { get; set; }
	public long MMSI_String { get; set; }
	public string ShipName { get; set; } = string.Empty;
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public DateTimeOffset time_utc { get; set; }
}
