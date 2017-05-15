using System;
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

            var mapSize = 128;
            var map = new Map(tags.Length, mapSize, 100, data);
            //map.DumpCoordinates();
            var resultMap = map.ResultMap();
            var bitmap = new Bitmap(mapSize, mapSize);
            var max = 0d;
            for(int x = 0; x < mapSize; x++) {
                for(int y = 0; y < mapSize; y++)
                {
                    var average = resultMap[x, y].Aggregate(0d, (acc, v) => acc + v);
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

            Console.Write("Save SOM output as image? (Y/N): ");
            var answer = Console.ReadLine();
            if (answer.ToLower() == "y")
            {
                bitmap.Save("./SOM.png", ImageFormat.Png);
            }
            else
            {
                var box = new PictureBox();
                box.Image = bitmap;
                box.Show();
            }
            Console.ReadLine();

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
