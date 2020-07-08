# 一致性Hash(ConsistentHashing)算法C#实现
demo for practice consistent hashing algorithm

## 基本概念
- 缓存服务器节点列表
- 服务器虚拟节点映射
- 数据对象key
- 一致性Hash环

## 基本操作
- 获取Hash值算法
- 添加服务器节点到一致性Hash环
- 删除服务器节点
- 查找最近服务器虚拟节点对应的物理服务器地址

## 一致性Hash环数据结构设计

- ConsistentHashing


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
    }

采用C#中的SortedDictionary<uint, string>来表示一致性hash环，hashkey有序从小到大排序，该数据结构中保存的是
所有物理服务器经过虚拟化映射到的所有虚拟服务器节点hash值及其对应的物理服务器地址。

K-V对象进行缓存查找时，将对象的key传入一致性hash环，通过`GetTargetServerNode`方法，使用经过改造的二分查找算法，
返回离这个key位置最近的虚拟node对应的物理服务器的地址。

## 性能测试

- 测试数据：1_000_000 K-V对象，测试中仅需要key值即可。
- 物理服务器台数：N = 10台
- 单台物理服务器对应的虚拟服务器个数：M = 200个

测试项分为：
- A. 固定单台物理服务器格式为10台，通过调整M = 100/150/200/250/300查看标准差变化
- B. 固定单台物理服务器对应的虚拟节点为200台，增加一台物理服务器，查看失效比例

A测试结果：
- N = 10, M = 100, 标准差结果：7777.8296
- N = 10, M = 150, 标准差结果：6415.0162
- N = 10, M = 200, 标准差结果：7993.9259
- N = 10, M = 250, 标准差结果：4266.7723
- N = 10, M = 300, 标准差结果：6412.0133

B测试结果：
- N = 10, M = 200, N + 1, 失效比例：8.24%