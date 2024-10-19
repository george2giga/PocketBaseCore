using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketBaseCore
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        // the format we want to use is 2022-01-01 10:00:00.123Z
        private const string _format = "yyyy-MM-dd HH:mm:ss.fffZ";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.ParseExact(reader.GetString(), _format, null);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }
}