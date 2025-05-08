using System.Text.Json;
using System.Text.Json.Serialization;

namespace NBTIS.Core.Utilities
{

        public class CustomStringJsonConverter : JsonConverter<string>
        {
            public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string? value = null;

                if (reader.TokenType == JsonTokenType.Number)
                {
                    // Handle integer and floating-point numbers
                    if (reader.TryGetInt64(out long l))
                    {
                        value = l.ToString();
                    }
                    else if (reader.TryGetDouble(out double d))
                    {
                        value = d.ToString();
                    }
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    value = reader.GetString();
                }
                else if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }
                else
                {
                    throw new JsonException($"Unexpected token parsing string. Expected String or Number, got {reader.TokenType}.");
                }

                // Trim the value if it's not null
                return value?.Trim();
            }

            public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
            {
                // Trim the value before writing, if it's not null
                writer.WriteStringValue(value?.Trim());
            }
        }


    //public class NumberToStringConverter : JsonConverter<string>
    //{
    //    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        if (reader.TokenType == JsonTokenType.Number)
    //        {
    //            // Handle integer and floating point numbers
    //            if (reader.TryGetInt64(out long l))
    //            {
    //                return l.ToString();
    //            }
    //            else if (reader.TryGetDouble(out double d))
    //            {
    //                return d.ToString();
    //            }
    //        }
    //        else if (reader.TokenType == JsonTokenType.String)
    //        {
    //            return reader.GetString();
    //        }

    //        throw new JsonException($"Unexpected token parsing string. Expected String or Number, got {reader.TokenType}.");
    //    }

    //    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    //    {
    //        writer.WriteStringValue(value);
    //    }
    //}



}
