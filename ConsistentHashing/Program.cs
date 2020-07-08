using System;
using System.Collections.Generic;

namespace ConsistentHashing
{
    class Program
    {
        private const int TestDataCount = 1_000_000;

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

        private static readonly IEnumerable<string> TestData = GenerateTestData(TestDataCount);
        
        static void Main(string[] args)
        {
            // 1. Init ConsistentHashingRing and generate virtual nodes on the ring.
            Console.WriteLine($"Generate server nodes start...");
            var consistentHashRing = new ConsistentHashing(ServerNodes, 200);
            consistentHashRing.InitServerNodesOnRing();
            Console.WriteLine($"Generate server nodes complete...");

            // 2. Add Caches and count how many caches on each server node.
            Console.WriteLine($"Generate cache key start...");
            consistentHashRing.GenerateCachesSummary(TestData);
            Console.WriteLine($"Generate cache key complete...");

            // 3. Calculate standard deviation and print result.
            consistentHashRing.CalculateStandardDeviation();
            consistentHashRing.PrintCacheSummary();
            
            // 4. Add server
            consistentHashRing.AddServer("10.189.0.12");
            consistentHashRing.GenerateCachesSummary(TestData);
            consistentHashRing.CalculateStandardDeviation();
            consistentHashRing.PrintCacheSummary();
        }

        private static IEnumerable<string> GenerateTestData(int quantity)
        {
            var data = new List<string>();
            for (var i = 0; i < quantity; i++)
            {
                var random = new Random();
                var key = random.Next(0, int.MaxValue) + "#test";
                data.Add(key);
                Console.WriteLine($"Generate test data: {key}");
            }

            return data;
        }
    }
}