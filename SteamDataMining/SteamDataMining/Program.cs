using System;

namespace SteamDataMining
{
    class Program
    {
        static void Main(string[] args)
        {
            var testItems = new List<string[]>();

            testItems.Add( new string[] { "RPG", "FPS", "FUNNY"});
            testItems.Add(new string[] { "ACTION", "FPS", "SERIOUS" });
            testItems.Add(new string[] { "ACTION", "RPG", "FUNNY" });
            testItems.Add(new string[] { "RPG", "FPS", "FUNNY", "BEAUTIFUL" });
            testItems.Add(new string[] { "ACTION", "RPG", "FUNNY" });
            testItems.Add(new string[] { "ACTION", "RPG", "SERIOUS" });
            testItems.Add(new string[] { "ACTION", "RPG", "FUNNY","SHORT" });
            testItems.Add(new string[] { "ACTION", "RPG", "FUNNY" });
            testItems.Add(new string[] { "ACTION", "RPG", "FUNNY" });
            testItems.Add(new string[] { "FPS", "ACTION", "SERIOUS","BEAUTIFUL" });
            testItems.Add(new string[] { "ADVENTURE", "RPG", "SHORT" });
            

            var result = Apriori.MineItemSets(testItems, 3);

            Console.WriteLine("Supported sets found:");
            foreach (var set in result)
            {
                foreach (var item in set)
                {
                    Console.Write(item+ " ");
                }
                Console.WriteLine(";");
            }

            Console.ReadLine();
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
