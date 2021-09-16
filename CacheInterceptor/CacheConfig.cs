namespace CacheInterceptor
{
    public abstract class CacheConfig
    {
        public string CacheKeyPrefix { get; set; }
        public int DefaultExpirationInSeconds { get; set; } = 30;
    }
}
