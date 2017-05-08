using System;

namespace SteamDataMining
{
    class Program
    {
        static void Main(string[] args)
        {
            DataItem[] data;
            string[] tags;
            Preprocessing.ReadJson("data.json", out data, out tags);

            Console.WriteLine("Number of items: " + data.Length);
            Console.WriteLine("Number of tags: " + tags.Length);

            var map = new Map(tags.Length, 64, data);
            map.DumpCoordinates();
        }
    }
}
