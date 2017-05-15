using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SteamDataMining
{
    public static class Preprocessing
    {
        public static void ReadJson(string filepath, out DataItem[] data, out string[] tags, bool filterOutliers = false)
        {
            var file = File.ReadAllText(filepath);
            file = file.Replace("[]", "{}");
            var items = JsonConvert.DeserializeObject<Dictionary<int, Item>>(file);

            var tagList = new List<string>();

            foreach (var item in items)
                tagList.AddRange(item.Value.tags.Keys.Where(tag => !tagList.Contains(tag)));

            data = items.Select(item => ConvertItem(item.Value, tagList)).ToArray();
            tags = tagList.ToArray();
        }

        public static DataItem ConvertItem(Item item, List<string> tagList)
        {
            return new DataItem
            {
                appid = item.appid,
                name = item.name,
                rank = item.score_rank,
                owners = item.owners,
                players = item.players_forever,
                price = item.price,
                tags = item.tags,
                ntags = item.tags.ToDictionary(tag => tagList.IndexOf(tag.Key), tag => tag.Value)
            };
        }

        public class Item
        {
            public int appid;
            public string name;
            public string developer;
            public string publisher;
            public int? score_rank;
            public int owners;
            public int owners_variance;
            public int players_forever;
            public int players_forever_variance;
            public int players_2weeks;
            public int players_2weeks_variance;
            public int average_forever;
            public int average_2weeks;
            public int median_forever;
            public int median_2weeks;
            public int ccu;
            public int? price;
            public Dictionary<string, int> tags;
        }
    }
}