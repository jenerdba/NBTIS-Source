using System.Text.Json;
using System.Text.Json.Serialization;

namespace NBTIS.Core.Utilities
{
    public class CustomNullableDoubleJsonConverter : JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                string? str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                {
                    return null;
                }
                if (double.TryParse(str, out double result))
                {
                    return result;
                }
                throw new JsonException($"Unable to convert \"{str}\" to a double.");
            }
            throw new JsonException($"Unexpected token {reader.TokenType} when parsing double.");
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
