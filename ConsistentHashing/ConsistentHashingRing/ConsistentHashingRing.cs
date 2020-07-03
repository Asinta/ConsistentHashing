using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsistentHashing.ConsistentHashingRing
{
    public class ConsistentHashingRing<TValue> : IConsistentHashingRing<TValue> where TValue : class
    {
        private readonly int _virtualNodeConunt;
        private readonly bool _showDebug;
        private readonly bool _showExecTime;
        private Stopwatch _watch;
        
        private Dictionary<int, TValue> _hashingRing;
        private Dictionary<string, List<int>> _nodeMapping;
        
        public ConsistentHashingRing(int virtualNodeConunt, bool showDebug = false, bool showExecTime = false)
        {
            _virtualNodeConunt = virtualNodeConunt;
            _showDebug = showDebug;
            _showExecTime = showExecTime;
            _watch = new Stopwatch();
            _hashingRing = new Dictionary<int, TValue>();
            _nodeMapping = new Dictionary<string, List<int>>();
        }

        public int GetHash(string key)
        {
            return key.GetHashCode();
        }

        public void AddServer(string serverKey)
        {
            _watch.Start();
            
            var generatedVirtualNodes = GenerateVirtualNodes(serverKey);
            foreach (var generatedVirtualNode in generatedVirtualNodes)
            {
                _hashingRing[generatedVirtualNode] = serverKey as TValue;
            }
            
            _watch.Stop();
            if (_showExecTime)
            {
                Console.WriteLine($"Add Server Elapse Time is: {_watch.Elapsed}");
            }
        }

        public void RemoveServer(string serverKey)
        {
            _watch.Start();
            
            foreach (var virtualNode in _nodeMapping[serverKey])
            {
                _hashingRing.Remove(virtualNode);
            }
            
            _watch.Stop();
            if (_showExecTime)
            {
                Console.WriteLine($"Remove Server Elapse Time is: {_watch.Elapsed}");
            }
        }

        public void AddObject(string key, TValue value)
        {
            _watch.Start();

            if (_hashingRing.ContainsKey(GetHash(key)))
            {
                Console.WriteLine($"{_hashingRing[GetHash(key)]}");
                throw new KeyNotFoundException("OH NO!!!!!!!!!!!!");
            }
            _hashingRing[GetHash(key)] = GetNearestNode(key);
            
            _watch.Stop();
            if (_showExecTime)
            {
                Console.WriteLine($"Add Object Elapse Time is: {_watch.Elapsed}");
            }
        }

        public TValue GetObject(string key)
        {
            return _hashingRing[GetHash(key)];
        }

        public int GetCountForKey(string key)
        {
            return _hashingRing.Values.Count(v => v.ToString() == key);
        }

        private TValue GetNearestNode(string key)
        {
            var keyHash = GetHash(key);
            var maxKeyList = _hashingRing.Keys.Where(k => k > keyHash).ToList();
            if (maxKeyList.Count == 0)
            {
                return _hashingRing[_hashingRing.Keys.Min()];
            }
            
            if (_showDebug)
            {
                Console.WriteLine($"Nearest Max Key is: {maxKeyList.Min()}");
            }
            return _hashingRing[maxKeyList.Min()];
        }

        private IList<int> GenerateVirtualNodes(string serverKey)
        {
            var generatedVirtualNodes = new List<int>();
            for (var virtualIndex = 1; virtualIndex <= _virtualNodeConunt; virtualIndex++)
            {
                var virtualHash = GetHash(serverKey + "#" + virtualIndex);
                generatedVirtualNodes.Add(virtualHash);
            }

            _nodeMapping[serverKey] = generatedVirtualNodes;

            if (_showDebug)
            {
                Console.WriteLine($"======================= Generated Virtual Nodes Start =======================");
                foreach (var virtualNode in _nodeMapping[serverKey])
                {
                    Console.WriteLine($"Generated Virtual Nodes: {virtualNode}");
                }
                Console.WriteLine($"======================= Generated Virtual Nodes End =======================");
            }
            
            return generatedVirtualNodes;
        }
    }
}