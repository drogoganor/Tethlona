using Cosmopolis.Data.Json;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Cosmopolis.Data
{
    public class Settings
    {
        [JsonConverter(typeof(JsonConverterVector2))]
        public Vector2 ScreenResolution { get; set; }
    }
}
