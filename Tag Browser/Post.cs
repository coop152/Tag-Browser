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
                JObject deserialised = JObject.Parse(json);
                PostNumber = (int)deserialised["id"];
                Artist = (string)deserialised["artist"];
                Tags = deserialised["tags"].ToObject<string[]>();
            }
            catch
            {
                PostNumber = 0;
                Artist = "No Artist";
                Tags = new string[0];
            }
        }
    }
}
