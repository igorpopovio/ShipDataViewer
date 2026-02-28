namespace ShipDataViewer.Core.Model;

public class Ship
{
	public int Mmsi { get; set; }
	public int ImoNumber { get; set; }
	public string Name { get; set; } = string.Empty;
	public string CallSign { get; set; } = string.Empty;
}
