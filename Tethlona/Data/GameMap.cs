using System;
using System.Numerics;
using System.Text.Json.Serialization;
using Tethlona.Data.Json;

namespace Tethlona.Data
{
    public class GameMap
    {
        public string Name { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonConverterVector3))]
        public Vector3 Size { get; set; }
        public MapBlock[] Blocks { get; set; } = Array.Empty<MapBlock>();
    }

    public class MapBlock
    {
        [JsonConverter(typeof(JsonConverterVector3))]
        public Vector3 Position { get; set; }
        public int[] FaceTextures { get; set; }
    }
}
