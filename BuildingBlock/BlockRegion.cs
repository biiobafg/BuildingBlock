using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BuildingBlock
{
    public class JsonConfig
    {
        public List<ushort> AffectedBarricades { get; set; } = new List<ushort>();
        public List<BlockRegion> BlockRegions { get; set; } = new List<BlockRegion>();

        public void Write(string levelName, string directory)
        {
            string filePath = Path.Combine(directory, $"{levelName}.json");
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        public static JsonConfig Load(string levelName, string directory)
        {
            string filePath = Path.Combine(directory, $"{levelName}.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<JsonConfig>(json);
            }
            return null;
        }
    }
    public class BlockRegion
    // way more complex than it should be but newtonsoft doesnt support vector3
    {
        [JsonIgnore]
        public Vector3 Position { get; set; }
        public float Distance { get; set; }

        [JsonProperty("x")]
        public float X
        {
            get => Position.x;
            set => Position=new(value, Position.y, Position.z);
        }
        [JsonProperty("y")]
        public float Y
        {
            get => Position.y;
            set => Position=new(Position.x, value, Position.z);
        }
        [JsonProperty("z")]
        public float Z
        {
            get => Position.z;
            set => Position=new(Position.x, Position.y, value);
        }

        public BlockRegion(Vector3 pos, float distance)
        {
            Position=pos;
            Distance=distance;
        }
        [JsonConstructor]
        public BlockRegion(float x, float y, float z, float distance)
        {
            Position=new Vector3(x, y, z);
            Distance=distance;
        }
    }
}
