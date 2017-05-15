﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace SteamDataMining
{
    public class ResultItem
    {
        public string tags;
        public double rating;
    }

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

            //var mapSize = 64;
            //var map = new Map(tags.Length, mapSize, data.Take(50).ToArray());
            ////map.DumpCoordinates();
            //var resultMap = map.ResultMap();

            //Console.Write("Save SOM output as image? (Y/N):");
            //var answer = Console.ReadLine();
            //if (answer.ToLower() == "y")
            //{

            //}
            //else
            //{
            //    var bitmap = new Bitmap(mapSize, mapSize);
            //    var max = 0d;
            //    for (int x = 0; x < mapSize; x++)
            //    {
            //        for (int y = 0; y < mapSize; y++)
            //        {
            //            var average = resultMap[x, y].Aggregate(0d, (acc, v) => acc + v) / tags.Length;
            //            if (average > max) max = average;
            //        }
            //    }
            //    for (int x = 0; x < mapSize; x++)
            //    {
            //        for (int y = 0; y < mapSize; y++)
            //        {
            //            var average = resultMap[x, y].Aggregate(0d, (acc, v) => acc + v) / max;
            //            bitmap.SetPixel(x, y, Color.FromArgb((int)(average * 255), (int)(average * 255), (int)(average * 255)));
            //        }
            //    }
            //    var box = new PictureBox();
            //    box.Image = bitmap;
            //}


            // ---------    APRIORI MINING          ----------
            var result = Apriori.MineItemSets(data.Select(x=>x.tags.Keys.ToArray()).ToList(), 0.03,3);


            var resultMappedToRating = result.ToDictionary(r => SetAsString(r), r => getAverage(data.Where(x => r.All(x.tags.Keys.Contains))));//.Average(d => d.rank));

            var asList = resultMappedToRating.ToList();
            asList.Sort((kv, kv2) => kv.Value.CompareTo(kv2.Value));

            WriteXML(asList.Select(a=>new ResultItem() {rating = a.Value,tags = a.Key.ToString()}).ToList(), @"c:\temp\test.xml");


            //Console.WriteLine("Supported sets of length "+ result.First().Count+" found:");
            foreach (var kv in asList)
            {
                Console.WriteLine(kv.Key + " : " + kv.Value);
            }

            Console.ReadLine();
        }

        private static string SetAsString(SortedSet<string> set)
        {
            var retString = "";

            foreach (var item in set)
            {
                retString += (item + " ");
            }
            return retString;
        }
        

        private static void WriteXML(List<ResultItem> tagsWithRating, string filename)
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(List<ResultItem>));

            System.IO.StreamWriter file = new System.IO.StreamWriter(
                filename);
            writer.Serialize(file, tagsWithRating);
            file.Close();
        }

        private static double getAverage(IEnumerable<DataItem> enumerable)
        {
            double total = 0;
            int count = 0;

            foreach (var dataItem in enumerable)
            {
                if (dataItem.rank != null)
                {
                    count++;
                    total += (int)dataItem.rank;
                }
            }
            return total / count;
        }
        
    }
    
}
