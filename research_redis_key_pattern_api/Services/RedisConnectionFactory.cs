using Microsoft.Extensions.Options;

using research_redis_key_pattern_api.Services.Contracts;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Services
{
    public class RedisConfiguration
    {
        public string ConnectionString { get; set; }
    }

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        /// <summary>
        /// The redis connection.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connection;

        public RedisConnectionFactory(IOptions<RedisConfiguration> redis)
        {
            this._connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redis.Value.ConnectionString));
        }

        public ConnectionMultiplexer Connection()
        {
            return this._connection.Value;
        }
    }
}
