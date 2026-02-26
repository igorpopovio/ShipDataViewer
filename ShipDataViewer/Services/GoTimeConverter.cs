using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShipDataViewer.Services;

public class GoTimeConverter : JsonConverter<DateTime>
{
	private const string Format = "yyyy-MM-dd HH:mm:ss.fffffffff";
	//                             2026-02-26 17:15:31.882688286

	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();

		if (DateTime.TryParseExact(
				value,
				Format,
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var dto))
		{
			return dto;
		}

		throw new JsonException($"Invalid date format: {value}");
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
	}
}
