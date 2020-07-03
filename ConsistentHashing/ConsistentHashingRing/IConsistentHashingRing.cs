using System.Collections.Generic;

namespace ConsistentHashing.ConsistentHashingRing
{
    public interface IConsistentHashingRing<TValue>
    {
        int GetHash(string key);
        
        void AddServer(string serverKey);
        void RemoveServer(string serverKey);
        
        void AddObject(string key, TValue value);
        TValue GetObject(string key);
    }
}