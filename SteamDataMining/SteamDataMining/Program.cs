using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

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

            var mapSize = 64;
            var map = new Map(tags.Length, mapSize, data.Take(50).ToArray());
            //map.DumpCoordinates();
            var resultMap = map.ResultMap();

            Console.Write("Save SOM output as image? (Y/N):");
            var answer = Console.ReadLine();
            if (answer.ToLower() == "y")
            {

            }
            else
            {
                var bitmap = new Bitmap(mapSize, mapSize);
                var max = 0d;
                for(int x = 0; x < mapSize; x++) {
                    for(int y = 0; y < mapSize; y++)
                    {
                        var average = resultMap[x, y].Aggregate(0d, (acc, v) => acc + v) / tags.Length;
                        if (average > max) max = average;
                    }
                }
                for(int x = 0; x < mapSize; x++) {
                    for(int y = 0; y < mapSize; y++)
                    {
                        var average = resultMap[x, y].Aggregate(0d, (acc, v) => acc + v) / max;
                        bitmap.SetPixel(x, y, Color.FromArgb((int) (average*255), (int) (average*255), (int) (average*255)));
                    }
                }
                var box = new PictureBox();
                box.Image = bitmap;
            }


            // ---------    APRIORI MINING          ----------
            var result = Apriori.MineItemSets(data.Select(x=>x.tags.Keys.ToArray()).ToList(), 500,3);

            Console.WriteLine("Supported sets of length "+ result.First().Count+" found:");
            foreach (var set in result)
            {
                foreach (var item in set)
                {
                    Console.Write(item+ " ");
                }
                Console.WriteLine(";");
            }

            Console.ReadLine();
        }
    }
}
