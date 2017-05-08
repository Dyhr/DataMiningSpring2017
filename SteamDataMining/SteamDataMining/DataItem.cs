using System.Collections.Generic;

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
    }
}