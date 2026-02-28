namespace ShipDataViewer.Core.Model;

public class Ship : IEquatable<Ship>
{
	public int Mmsi { get; set; }
	public int ImoNumber { get; set; }
	public string Name { get; set; } = string.Empty;
	public string CallSign { get; set; } = string.Empty;

	public bool Equals(Ship? other)
	{
		return other is not null && Mmsi == other.Mmsi;
	}
}
