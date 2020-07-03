using System;
using System.Collections.Generic;
using System.Globalization;
using ConsistentHashing.ConsistentHashingRing;

namespace ConsistentHashing
{
    class Program
    {
        private static readonly List<string> ServerNodes = new List<string>
        {
            "10.189.0.2",
            "10.189.0.3",
            "10.189.0.4",
            "10.189.0.5",
            "10.189.0.6",
            "10.189.0.7",
            "10.189.0.8",
            "10.189.0.9",
            "10.189.0.10",
            "10.189.0.11"
        };
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var consistentHashingRing = new ConsistentHashingRing<string>(20, true);
            foreach (var serverNode in ServerNodes)
            {
                consistentHashingRing.AddServer(serverNode);
            }

            var testData = GenerateTestKeyvalueData(100000);
            foreach (var (key, value) in testData)
            {
                consistentHashingRing.AddObject(key, value);
                Console.WriteLine($"Index: {key}");
            }

            var cacheMapping = GenerateCacheMapping(consistentHashingRing, ServerNodes);
            foreach (var (key, value) in cacheMapping)
            {
                Console.WriteLine($"{value} caches goes to server {key}");
            }
            
            
        }

        private static Dictionary<string, int> GenerateCacheMapping(ConsistentHashingRing<string> consistentHashingRing, List<string> serverNodes)
        {
            var mapping = new Dictionary<string, int>();
            foreach (var serverNode in serverNodes)
            {
                mapping[serverNode] = consistentHashingRing.GetCountForKey(serverNode);
            }

            return mapping;
        }

        static SortedDictionary<string, string> GenerateTestKeyvalueData(ulong quantity)
        {
            var data = new SortedDictionary<string, string>();
            for (ulong i = 0; i < quantity; i++)
            {
                data[i.ToString()] = (i + 0.1f).ToString(CultureInfo.CurrentCulture);
            }

            return data;
        }
    }
}