using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.IsolatedStorage;
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

        public static DataItem[] data;

        static void Main(string[] args)
        {

            // --------     DATA PROCESSING         ---------

            
            string[] tags;
            Preprocessing.ReadJson("data.json", out data, out tags);

            Console.WriteLine("Number of items: " + data.Length);
            Console.WriteLine("Number of tags: " + tags.Length);
            Console.WriteLine();
            
            // ---------    APRIORI MINING          ----------
            double threshold = 0.02;
            double confidence = 0.35;

            var result = Apriori.MineItemSets(data.Select(x => x.tags.Keys.ToArray()).ToList(), threshold, 10,confidence, true);

            var resultMappedToRating = result.ToDictionary(r => SetAsString(r),
                r => getAverage(data.Where(x => r.All(x.tags.Keys.Contains)))); //.Average(d => d.rank));


            var xs =
                result.Select(
                    r =>
                        new ResultItem()
                        {
                            rating = getAverage(data.Where(x => r.All(x.tags.Keys.Contains))),
                            tags = SetAsString(r),
                            median = getMedian(data.Where(x => r.All(x.tags.Keys.Contains)))
                        }).ToList();


            xs.Sort((kv, kv2) => kv.rating.CompareTo(kv2.rating));

            WriteXML(xs, @"./th" + threshold + ".xml");


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


            // --------     SELF-ORGANIZING MAPS    ---------

            Console.WriteLine();
            Console.WriteLine("Training a SOM on items with frequent tagsets.");

            var rnd = new Random();
            var frequentData = data.Where(item => rnd.NextDouble() < 0.05 &&
                                                  result.Any(set => set.All(tag => item.tags.ContainsKey(tag)))).ToArray();
            Console.WriteLine("Training with "+frequentData.Length+ " items");

            var mapSize = (int)Math.Ceiling(Math.Sqrt(frequentData.Length))*3;
            Console.WriteLine("Using map size of "+mapSize);
            var map = new Map(tags.Length, mapSize, 100, frequentData);
            var resultMap = map.ResultMap();
            var bitmap = new Bitmap(mapSize, mapSize);
            var colors = tags.Select((_, i) => Color.FromArgb(rnd.Next(255),rnd.Next(255),rnd.Next(255))).ToArray();
            for(int i = 0; i < colors.Length; i++)
                Console.WriteLine($"{tags[i]}: {colors[i].R},{colors[i].G},{colors[i].B} - {colors[i].Name}");
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    bitmap.SetPixel(x, y, GetColor(colors, resultMap, x, y));  // Change the GetColor function to color the landscape
                }
            }
            for (var i = 0; i < map.patterns.Count; i++)
            {
                var pos = map.Winner(map.patterns[i]);
                bitmap.SetPixel(pos.Item1, pos.Item2, Color.Black);  // Change this part to color the datapoints
            }
            bitmap.Save("./SOM.png", ImageFormat.Png);
            Console.WriteLine("Done");
        }

        //should be sorted before.
        private static int getMedian(IEnumerable<DataItem> enumerable)
        {
            var cs = enumerable.Where(d => d.rank != null).ToList();
            
            cs.Sort((d,d1)=>((int)d.rank).CompareTo((int)d1.rank));

            return (int)cs.ElementAt((cs.Count/2)).rank;
        }

        private static Color GetColor(Color[] colors, double[,][] data, int x, int y)
        {
            double[] item = data[x, y];
            double[] color = {0.0, 0.0, 0.0};
            int total = 0;

            // Blend the color of the tags
            for (int i = 0; i < item.Length; i++)
            {
                color[0] += colors[i].R * item[i];
                color[1] += colors[i].G * item[i];
                color[2] += colors[i].B * item[i];
                total ++;
            }

            color[0] /= total;
            color[1] /= total;
            color[2] /= total;

            // Make sure we're not out of bounds
            var max = Math.Max(color.Max(), 255);
            color[0] = color[0]/max * 255;
            color[1] = color[1]/max * 255;
            color[2] = color[2]/max * 255;
            if (color[0] < 0) color[0] = 0;
            if (color[1] < 0) color[1] = 0;
            if (color[2] < 0) color[2] = 0;
            // This shouldn't be necessary, but just in case.
            if (color[0] > 255) color[0] = 255;
            if (color[1] > 255) color[1] = 255;
            if (color[2] > 255) color[2] = 255;

            return Color.FromArgb((int) color[0], (int) color[1], (int) color[2]);
        }
        private static double Difference(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += Math.Abs(a[i] - b[i]);
            return sum;
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
