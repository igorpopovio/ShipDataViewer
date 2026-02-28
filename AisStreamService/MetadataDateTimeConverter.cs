using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShipDataViewer.Services;

public class MetadataDateTimeConverter : JsonConverter<DateTimeOffset>
{
	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString()?.Replace(" UTC", "") ?? string.Empty;

		var datetime = DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

		if (DateTimeOffset.TryParse(
				value,
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal,
				out var dto))
		{
			return dto;
		}

		throw new JsonException($"Invalid date format: {value}");
	}

	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
