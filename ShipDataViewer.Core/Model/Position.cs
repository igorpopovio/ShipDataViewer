namespace ShipDataViewer.Core.Model;

public class Position
{
	public int ShipMmsi { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public double Sog { get; set; }
	public double Cog { get; set; }
	public int TrueHeading { get; set; }
}
