using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamDataMining
{
    public class DataItem
    {
        public int appid;
        public string name;
        public int? rank;
        public int owners;
        public int players;
        public int? price;
        public Dictionary<string, int> tags;
        public Dictionary<int, int> ntags;

        public double[] vectorize(int dimensions)
        {
            var result = new double[dimensions];
            foreach (var tag in ntags)
                result[tag.Key] = 1.0;
            return result;
        }
    }
}