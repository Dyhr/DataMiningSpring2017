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
        public static List<SortedSet<string>> MineItemSets(List<string[]> data, double supportThreshold, int minimumPrintSize)
        {
            Console.WriteLine("Running Apriori using support threshold " + supportThreshold);
            int k;
            List<SortedSet<string>> supportedCandidates = new List<SortedSet<string>>();

            //double pct = 100 * supportThreshold / data.Count;
            int supportCount = (int) (supportThreshold * data.Count);

            Dictionary<SortedSet<string>, int> frequentItemSets = generateFrequentItemSetsLevel1(data, supportCount);

            Console.WriteLine("Found " + frequentItemSets.Count + " supported length 1 patterns");

            supportedCandidates = new List<SortedSet<string>>();
            for (k = 1; frequentItemSets.Count > 0; k++)
            {

                Console.WriteLine("Finding frequent itemsets of length " + (k + 1) + " ...");
                frequentItemSets = generateFrequentItemSets(supportCount, data, frequentItemSets);
                
                Console.WriteLine(" - found " + frequentItemSets.Count + " supported candidates.");

                if (k + 1 >= minimumPrintSize)
                {

                    supportedCandidates.AddRange(frequentItemSets.Keys);

                    foreach (var set in frequentItemSets.Keys)
                    {
                        foreach (var item in set)
                        {
                            Console.Write(item + " ");
                        }
                        Console.WriteLine(";");
                    }
                }   
            }

            //RuleGeneration(supportedCandidates,data);

            return supportedCandidates;
        }

        private static void RuleGeneration(List<SortedSet<string>> supportedCandidates, List<string[]> data)
        {
            throw new NotImplementedException();
        }
        

        private static Dictionary<SortedSet<string>, int> generateFrequentItemSets(int supportThreshold, List<string[]> data, Dictionary<SortedSet<string>, int> lowerLevelItemSets)
        {
            Dictionary<SortedSet<string>, int> candidates = new Dictionary<SortedSet<string>, int>();

            // generate candidate itemsets from the lower level itemsets
            foreach (SortedSet<string> itemSet1 in lowerLevelItemSets.Keys)
            {
                foreach (SortedSet<string> itemSet2 in lowerLevelItemSets.Keys)
                {
                    if (!itemSet1.All(itemSet2.Contains) && almostEqual(itemSet1,itemSet2)) //not the same and first k-1 elements are equal
                    {
                        SortedSet<string> newSet = joinSets(itemSet1, itemSet2);

                        if (newSet.Count == itemSet1.Count + 1) // if they had k-1 elements in common
                        {
                            int i = 0;
                            //Pruning
                            while (i < newSet.Count)
                            {
                                var e = newSet.ElementAt(i);

                                SortedSet<string> subset = new SortedSet<string>(newSet.Where(x=> !x.Equals(e)));

                                if(!lowerLevelItemSets.Any(kv=> subset.All(kv.Key.Contains)))
                                    break;
                                i++;

                            }
                            //if all subsets are in the lower level sets
                            if(i== newSet.Count)
                                if (!candidates.Any(c => newSet.All(s => c.Key.Contains(s))))
                                    candidates[newSet] = 0;
                        }
                    }
                }
            }
            Console.WriteLine("generated " + candidates.Count + " candidates.");

            //check the support for all candidates and returns only those that have enough support to the set
            return candidates.ToDictionary(kv => kv.Key, kv => countSupport(kv.Key, data)).Where(c => c.Value >= supportThreshold).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static bool almostEqual(SortedSet<string> itemSet1, SortedSet<string> itemSet2)
        {
            var k = itemSet1.Count;

            for (int i = 0; i < k-1; i++)
            {
                if (!itemSet1.ElementAt(i).Equals(itemSet2.ElementAt(i)))
                    return false;
            }
            return true;
        }

        private static SortedSet<string> joinSets(SortedSet<string> first, SortedSet<string> second)
        {
            var items = new SortedSet<string>();
            foreach (var s in first)
                items.Add(s);
            foreach (var s in second)
                items.Add(s);

            items.ToList();

            return items;
        }

        //return frequent 1-itemsets mapped to the support count
        private static Dictionary<SortedSet<string>, int> generateFrequentItemSetsLevel1(List<string[]> data, int supportThreshold)
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

            return returnTable.Where(n => n.Value >= supportThreshold).ToDictionary(kv => new SortedSet<string>() { kv.Key }, kv => kv.Value);
        }

        //returns how many tuples in our data set, that contains all items of the given set.
        // Assumes that items in ItemSets and transactions are both unique
        private static int countSupport(SortedSet<string> itemSet, List<string[]> data)
            => data.Where(item => itemSet.All(str => item.Contains(str))).Count();

    }
}
