using System.ComponentModel;

namespace ShipDataViewer.Core.Model;

public partial class Ship : IEquatable<Ship>, INotifyPropertyChanged
{
	public int Mmsi { get; set; }
	public int ImoNumber { get; set; }
	public string Name { get; set; } = string.Empty;
	public string CallSign { get; set; } = string.Empty;
	public Position? LastReportedPosition { get; set; }
	public DateTime LastUpdated { get; set; }

	public bool Equals(Ship? other)
	{
		return other is not null && Mmsi == other.Mmsi;
	}

	public override bool Equals(object? obj) => Equals(obj as Ship);

	public override int GetHashCode() => Mmsi.GetHashCode();

	public static bool operator ==(Ship ship1, Ship ship2)
	{
		if (ship1 is null)
		{
			return ship2 is null;
		}

		return ship1.Equals(ship2);
	}

	public static bool operator !=(Ship ship1, Ship ship2)
	{
		if (ship1 is null)
		{
			return ship2 is not null;
		}

		return !ship1.Equals(ship2);
	}
}
