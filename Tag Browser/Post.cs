using System;
using System.IO;

namespace Tag_Browser
{
    public class Post
    {
        public string Path { get; set; }
        public string[] Tags { get; set; }
        public string Artist
        {
            get
            {
                string[] directories = Path.Split(System.IO.Path.DirectorySeparatorChar);
                string folderName = directories[directories.Length - 2];
                if (folderName.EndsWith("-q"))
                {
                    folderName = folderName.Remove(folderName.Length - 2);
                }

                return folderName;
            }
        }
        public int PostNumber
        {
            get
            {
                try
                {
                    int indexSlash = Path.LastIndexOf('\\');
                    int indexDot = Path.LastIndexOf('.');
                    int diff = indexDot - indexSlash - 1;
                    return int.Parse(Path.Substring(indexSlash + 1, diff));
                }
                catch { return 0; }
            }
        }
        public Post(string path, string[] tags)
        {
            Path = path;
            Tags = tags;
        }
        public Post(string path)
        {
            Path = path;
            try
            {
                Tags = File.ReadAllLines(path + ".txt");
            }
            catch
            {
                Tags = new string[0];
            }
        }
    }
}
