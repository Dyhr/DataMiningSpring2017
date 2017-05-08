using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamDataMining
{
    class Program
    {
        static void Main(string[] args)
        {

            // --------     DATA PROCESSING         ---------

            DataItem[] data;
            string[] tags;
            Preprocessing.ReadJson("data.json", out data, out tags);

            Console.WriteLine("Number of items: " + data.Length);
            Console.WriteLine("Number of tags: " + tags.Length);

            // --------     SELF-ORGANIZING MAPS    ---------

            //var map = new Map(tags.Length, 64, data);
            //map.DumpCoordinates();
            


            // ---------    APRIORI MINING          ----------
            var result = Apriori.MineItemSets(data.Select(x=>x.tags.Keys.ToArray()).ToList(), 50);

            Console.WriteLine("Supported sets found:");
            foreach (var set in result)
            {
                foreach (var item in set)
                {
                    Console.Write(item+ " ");
                }
                Console.WriteLine(";");
            }
            
        }
    }
}
