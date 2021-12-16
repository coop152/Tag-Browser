using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tag_Browser
{
    public class Post
    {
        public string Path { get; set; }
        public string[] Tags { get; set; }
        public string Artist { get; set; }
        public int PostNumber { get; set; }
        public Post(string path)
        {
            Path = path;
            try
            {
                string json = File.ReadAllText(path + ".json");
                JObject deserialised = JsonConvert.DeserializeObject<JObject>(json);
                Artist = (string)deserialised["artist"];
                Tags = deserialised["tags"].ToObject<string[]>();
                PostNumber = (int)deserialised["id"];
            }
            catch
            {
                Tags = new string[0];
            }
        }
    }
}
