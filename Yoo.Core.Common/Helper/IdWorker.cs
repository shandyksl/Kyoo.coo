using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Common.Helper
{
    public class IdWorker
    {
        private static IdWorker _instance;
        private static readonly object LockObject = new object();

        private long _lastTimestamp = -1L;
        private readonly long _workerIdBits = 5L;
        private readonly long _datacenterIdBits = 5L;
        private readonly long _maxWorkerId = -1L ^ (-1L << 5);
        private readonly long _maxDatacenterId = -1L ^ (-1L << 5);
        private readonly long _sequenceBits = 12L;
        private readonly long _workerIdShift = 12L;
        private readonly long _datacenterIdShift = 17L;
        private readonly long _timestampLeftShift = 22L;
        private readonly long _sequenceMask = -1L ^ (-1L << 12);

        private long _sequence = 0L;
        private readonly long _datacenterId;
        private readonly long _workerId;

        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 私有构造函数，防止直接实例化
        private IdWorker(long workerId, long datacenterId)
        {
            // 初始化 IdWorker 的逻辑
            if (workerId > _maxWorkerId || workerId < 0)
            {
                throw new ArgumentException($"Worker ID must be between 0 and {_maxWorkerId}");
            }

            if (datacenterId > _maxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException($"Datacenter ID must be between 0 and {_maxDatacenterId}");
            }

            _workerId = workerId;
            _datacenterId = datacenterId;
        }

        public static IdWorker Instance
        {
            get
            {
                // 使用双重锁定以确保线程安全
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            // 这里可以根据需要传递不同的参数
                            _instance = new IdWorker(1, 1);
                        }
                    }
                }

                return _instance;
            }
        }

        public long NextId()
        {
            // 生成唯一的 ID 的逻辑
            lock (LockObject)
            {
                long timestamp = (long)(DateTime.UtcNow - _epoch).TotalMilliseconds;

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & _sequenceMask;
                    if (_sequence == 0)
                    {
                        // Sequence overflow, wait until next millisecond.
                        timestamp = UntilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0L;
                }

                if (timestamp < _lastTimestamp)
                {
                    throw new InvalidOperationException($"Clock moved backwards. Refusing to generate ID for {_lastTimestamp - timestamp} milliseconds.");
                }

                _lastTimestamp = timestamp;

                return ((timestamp - _epoch.Ticks) << (int)_timestampLeftShift)
                       | (_datacenterId << (int)_datacenterIdShift)
                       | (_workerId << (int)_workerIdShift)
                       | _sequence;
            }
        }

        private static long UntilNextMillis(long lastTimestamp)
        {
            long timestamp = (long)(DateTime.UtcNow - _epoch).TotalMilliseconds;
            while (timestamp <= lastTimestamp)
            {
                timestamp = (long)(DateTime.UtcNow - _epoch).TotalMilliseconds;
            }
            return timestamp;
        }
    }
}
