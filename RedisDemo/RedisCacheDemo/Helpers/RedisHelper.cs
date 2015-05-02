using StackExchange.Redis;
using System.Configuration;

namespace RedisCacheDemo.Helpers
{
    public static class RedisHelper
    {
        private static ConnectionMultiplexer redis;
        
        public static void Start()
        {
            // El string de conexión a redis está almacenado en el web.config.
            var server = ConfigurationManager.AppSettings.Get("RedisDatabase");

            // El ConnectionMultiplexer está diseñado para ser compartido y reutilizado, 
            // por eso solo se inicializa una sola vez.
            // Ver: https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Basics.md
            redis = ConnectionMultiplexer.Connect(server);
        }

        public static void Stop()
        {
            if (redis != null)
            {
                redis.Close();
                redis.Dispose();
            }
        }

        public static IDatabase GetDatabase()
        {
            return redis.GetDatabase();
        }
    }
}