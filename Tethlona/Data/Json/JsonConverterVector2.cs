using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tethlona.Data.Json
{
    public class JsonConverterVector2 : JsonConverter<Vector2>
    {
        public override Vector2 Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var allValuesRead = false;
            var valueArray = new float[2];
            var index = 0;
            while (!allValuesRead && reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray:
                        allValuesRead = true;
                        break;
                    case JsonTokenType.Number:
                        valueArray[index] = reader.GetInt32();
                        break;
                }

                index++;
            }

            return new Vector2(valueArray[0], valueArray[1]);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Vector2 value,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}
