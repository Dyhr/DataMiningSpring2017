using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace SteamDataMining
{
    public class ResultItem
    {
        public string tags;
        public double rating;
        public int median;
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
            Console.WriteLine();
            
            // ---------    APRIORI MINING          ----------
            double threshold = 0.03;
            double confidence = 0.30;

            while (threshold > 0.02)
            {
                var result = Apriori.MineItemSets(data.Select(x => x.tags.Keys.ToArray()).ToList(), threshold, 3,confidence, true);

                var resultMappedToRating = result.ToDictionary(r => Apriori.SSetToString(r),
                    r => getAverage(data.Where(x => r.All(x.tags.Keys.Contains)))); //.Average(d => d.rank));


                var xs =
                    result.Select(
                        r =>
                            new ResultItem()
                            {
                                rating = getAverage(data.Where(x => r.All(x.tags.Keys.Contains))),
                                tags = Apriori.SSetToString(r),
                                median = getMedian(data.Where(x => r.All(x.tags.Keys.Contains)))
                            }).ToList();


                xs.Sort((kv, kv2) => kv.rating.CompareTo(kv2.rating));

                WriteXML(xs,
                    @"./th" + threshold.ToString() + ".xml");
                threshold -= 0.01;

            }

            //AVERAGE RATINGS PR TAG
            var tagRatings = tags.Select(r => new ResultItem()
            {
                rating = getAverage(data.Where(x => x.tags.Keys.Contains(r))),
                median = getMedian(data.Where(x => x.tags.Keys.Contains(r))),
                tags = r
            }).ToList();

            tagRatings.Sort((kv, kv2) => kv.rating.CompareTo(kv2.rating));

            WriteXML(tagRatings,
                @"./tagRatings.xml");

            Console.ReadLine();


            // --------     SELF-ORGANIZING MAPS    ---------

            var mapSize = 128;
            var map = new Map(tags.Length, mapSize, 100, data);
            //map.DumpCoordinates();
            var resultMap = map.ResultMap();
            var bitmap = new Bitmap(mapSize, mapSize);
            var max = 0d;
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    var average = resultMap[x, y].Aggregate(0d, (acc, v) => acc + v);
                    if (average > max) max = average;
                }
            }
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    var average = resultMap[x, y].Aggregate(0d, (acc, v) => acc + v) / max;
                    bitmap.SetPixel(x, y, Color.FromArgb((int)(average * 255), (int)(average * 255), (int)(average * 255)));
                }
            }
            bitmap.Save("./SOM.png", ImageFormat.Png);

            Console.ReadLine();
        }

        //should be sorted before.
        private static int getMedian(IEnumerable<DataItem> enumerable)
        {
            var cs = enumerable.Where(d => d.rank != null).ToList();
            
            cs.Sort((d,d1)=>((int)d.rank).CompareTo((int)d1.rank));

            return (int)cs.ElementAt((cs.Count/2)).rank;
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
