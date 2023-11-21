using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.Chat
{
       internal class RateLimiter
    {
        // TODO: Move this to CCVars.cs
        // Time in milliseconds at which rate user is being limited
        public static readonly int RateDelta = 750;

        private struct LimitRecord
        {
            public long PingTimestamp;

            // TODO: Use this variable to suppress future network notification
            // to avoid flood
            //public bool WasNotified = false;
        }
        private static Dictionary<string, LimitRecord> _list = new();
        private static RateLimiter? _instance;

        private RateLimiter()
        { }

        public static RateLimiter GetInstance()
        {
            if (_instance == null)
                _instance = new RateLimiter();
            return _instance;
        }

        public static bool IsBeingRateLimited(string playerId, bool renew = true)
        {
            long currentTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (_list.TryGetValue(playerId, out LimitRecord value) == false)
            {
                if (renew)
                {
                    _list.Add(playerId, new LimitRecord()
                    {
                        PingTimestamp = currentTimestamp + RateDelta
                    });
                }
                return false;
            }

            if (value.PingTimestamp >= currentTimestamp)
            {
                return true;
            }

            if (renew)
            {
                value.PingTimestamp = currentTimestamp + RateDelta;
                _list[playerId] = value;
            }

            return false;
        }
    }
}
