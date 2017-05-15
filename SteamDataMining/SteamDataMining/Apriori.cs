using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SteamDataMining
{
    class Apriori
    {
        public static List<HashSet<string>> MineItemSets(List<string[]> data, int supportThreshold, int minimumPrintSize)
        {
            Console.WriteLine("Running Apriori using support threshold " + supportThreshold);
            int k;
            List<HashSet<string>> supportedCandidates = new List<HashSet<string>>();

            double pct = 100 * supportThreshold / data.Count;

            Dictionary<HashSet<string>, int> frequentItemSets = generateFrequentItemSetsLevel1(data, supportThreshold);

            Console.WriteLine("Found " + frequentItemSets.Count + " supported length 1 patterns");

            for (k = 1; frequentItemSets.Count > 0; k++)
            {

                supportedCandidates = new List<HashSet<string>>(frequentItemSets.Keys);

                Console.WriteLine("Finding frequent itemsets of length " + (k + 1) + " ...");
                frequentItemSets = generateFrequentItemSets(supportThreshold, data, frequentItemSets);


                Console.WriteLine(" found " + frequentItemSets.Count);

                if (k + 1 >= minimumPrintSize)
                    foreach (var set in frequentItemSets.Keys)
                    {
                        foreach (var item in set)
                        {
                            Console.Write(item + " ");
                        }
                        Console.WriteLine(";");
                    }
            }

            return supportedCandidates;
        }

        private static Dictionary<HashSet<string>, int> generateFrequentItemSets(int supportThreshold, List<string[]> data, Dictionary<HashSet<string>, int> lowerLevelItemSets)
        {
            Dictionary<HashSet<string>, int> candidates = new Dictionary<HashSet<string>, int>();

            // generate candidate itemsets from the lower level itemsets
            foreach (HashSet<string> itemSet1 in lowerLevelItemSets.Keys)
            {
                foreach (HashSet<string> itemSet2 in lowerLevelItemSets.Keys)
                {
                    if (!itemSet1.Equals(itemSet2))
                    {
                        HashSet<string> newSet = joinSets(itemSet1, itemSet2);



                        if (newSet.Count == itemSet1.Count + 1) // if they had k-1 elements in common
                            if (!candidates.Any(c => newSet.All(s => c.Key.Contains(s))))
                                candidates[newSet] = 0;
                    }
                }
            }
            Console.WriteLine("generated " + candidates.Count + " candidates.");

            //check the support for all candidates and returns only those that have enough support to the set
            return candidates.ToDictionary(kv => kv.Key, kv => countSupport(kv.Key, data)).Where(c => c.Value >= supportThreshold).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static HashSet<string> joinSets(HashSet<string> first, HashSet<string> second)
        {
            var items = new HashSet<string>();
            foreach (var s in first)
                items.Add(s);
            foreach (var s in second)
                items.Add(s);

            return items;
        }

        //return frequent 1-itemsets mapped to the support count
        private static Dictionary<HashSet<string>, int> generateFrequentItemSetsLevel1(List<string[]> data, int supportThreshold)
        {
            Dictionary<string, int> returnTable = new Dictionary<string, int>();

            foreach (var ix in data)
            {
                foreach (var i in ix)
                {

                    if (returnTable.ContainsKey(i))
                    {
                        returnTable[i]++;
                    }
                    else
                    {
                        returnTable.Add(i, 1);
                    }
                }
            }

            return returnTable.Where(n => n.Value >= supportThreshold).ToDictionary(kv => new HashSet<string>() { kv.Key }, kv => kv.Value);
        }

        //returns how many tuples in our data set, that contains all items of the given set.
        // Assumes that items in ItemSets and transactions are both unique
        private static int countSupport(HashSet<string> itemSet, List<string[]> data)
            => data.Where(item => itemSet.All(str => item.Contains(str))).Count();

    }
}
