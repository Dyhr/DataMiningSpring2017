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
            var result = Apriori.MineItemSets(data.Select(x=>x.tags.Keys.ToArray()).ToList(), 0.03,3);


            var resultMappedToRating = result.ToDictionary(r => r, r => data.Where(x => r.All(x.tags.Keys.Contains)).Average(d => d.rank));

            

            //Console.WriteLine("Supported sets of length "+ result.First().Count+" found:");
            foreach (var set in resultMappedToRating)
            {
                foreach (var item in set.Key)
                {
                    Console.Write(item + " ");
                }
                Console.Write(set.Value);
                Console.WriteLine(";");
            }

            Console.ReadLine();
        }
    }
}
