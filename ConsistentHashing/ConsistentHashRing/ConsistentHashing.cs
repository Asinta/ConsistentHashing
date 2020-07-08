using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ConsistentHashing.ConsistentHashRing
{
    public class ConsistentHashing
    {
        private readonly IEnumerable<string> _serversList;
        private readonly int _virtualNodesCount;

        private SortedDictionary<uint, string> _hashRing;
        private List<IGrouping<string, int>> _cacheSummary;

        private double _stdDeviation;
        private double _meanCacheCount;

        public ConsistentHashing(IEnumerable<string> serversList, int virtualNodesCount)
        {
            _serversList = serversList;
            _virtualNodesCount = virtualNodesCount;
            _hashRing = new SortedDictionary<uint, string>();
            _cacheSummary = new List<IGrouping<string, int>>();
        }

        public void InitServerNodesOnRing()
        {
            foreach (var server in _serversList)
            {
                var virtualNodesHash = ConsistentHashingUtil.ComputeVirtualNodesHash(server, _virtualNodesCount);
                foreach (var virtualHash in virtualNodesHash)
                {
                    if (_hashRing.ContainsKey(virtualHash))
                    {
                        Console.WriteLine($"Oooops, already has a key {virtualHash}");
                    }
                    _hashRing.Add(virtualHash, server);
                }
            }
        }

        private string GetTargetServerNode(string key)
        {
            var hashValue = ConsistentHashingUtil.ComputeHash(key);
            var serverNode = ConsistentHashingUtil.ConsistentHashingRingBinarySearch(_hashRing.Keys.ToArray(), hashValue);
            
            return _hashRing[serverNode];
        }

        public void CalculateStandardDeviation()
        {
            var countList = _cacheSummary.Select(cache => cache.Count()).ToArray();
            _meanCacheCount = countList.Average();
            
            var deviationSum = countList.Sum(c => (c - _meanCacheCount) * (c - _meanCacheCount));
            var count = countList.Length - 1;

            _stdDeviation = count > 0.0 ? Math.Round(Math.Sqrt(deviationSum / count), 5) : -1;
        }

        public void PrintCacheSummary()
        {
            Console.WriteLine("============================> CACHE SUMMARY: ");
            Console.WriteLine($"-> Std Deviation: {_stdDeviation} under {_cacheSummary.Sum(c => c.Count())} " +
                              $"with virtual nodes {_virtualNodesCount} each for {_serversList.Count()} physical server nodes.");
            Console.WriteLine($"-> Mean Cache Count: {_meanCacheCount}");
            Console.WriteLine("============================> CACHE DISTRIBUTE: ");
            _cacheSummary.ForEach(cache =>
            {
                Console.WriteLine($"{cache.Key} has cache count {cache.Count()}");
            });
        }

        public void GenerateCachesSummary(IEnumerable<string> testData)
        {
            var nodesSummary = testData.Select(GetTargetServerNode).ToList();
            _cacheSummary = nodesSummary.GroupBy(s => s, s => s.Count()).ToList();
        }
    }

    public static class ConsistentHashingUtil
    {
        public static uint ComputeHash(string key)
        {
            using (var hashAlgorithm = MD5.Create())
            {
                var hashValue = BitConverter.ToUInt32(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(key)));
                return hashValue;
            }
        }

        public static IEnumerable<uint> ComputeVirtualNodesHash(string key, int virtualNodesCount = 100)
        {
            using (var hashAlgorithm = MD5.Create())
            {
                var virtualNodesHash = new List<uint>();
                for (var count = 1; count <= virtualNodesCount; count++)
                {
                    var identifier = key.GetHashCode() + "#" + count;
                    var hashValue = BitConverter.ToUInt32(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(identifier)));
                    virtualNodesHash.Add(hashValue);
                }
                return virtualNodesHash;
            }
        }

        public static uint ConsistentHashingRingBinarySearch(uint[] ring, uint searchKey)
        {
            var min = 0;
            var max = ring.Length - 1;
            
            if (searchKey < ring[min] || searchKey > ring[max])
                return ring[0];
            
            while (max - min > 1)
            {
                var mid = (max + min) / 2;
                if (ring[mid] >= searchKey)
                {
                    max = mid;
                }
                else
                {
                    min = mid;
                }
            }

            return ring[max];
        }
    }
}