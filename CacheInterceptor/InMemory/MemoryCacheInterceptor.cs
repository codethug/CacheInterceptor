using Microsoft.Extensions.Caching.Memory;

namespace CacheInterceptor.InMemory
{
    public class MemoryCacheInterceptor : CacheInterceptor<CacheConfig>
    {
        private IMemoryCache _memoryCache;
        public MemoryCacheInterceptor(IMemoryCache memoryCache, MemoryCacheConfig config) : base(config)
        {
            _memoryCache = memoryCache;
        }

        public override bool TryGetCacheValue<TResult>(string cacheKey, out TResult cacheValue)
        {
            return _memoryCache.TryGetValue(cacheKey, out cacheValue);
        }

        public override void SetCacheValue<TItem>(string cacheKey, TItem value, CacheAttribute cacheOptions)
        {
            var memoryCacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    new System.TimeSpan(hours: 0, minutes: 0,
                        seconds: cacheOptions.Seconds ?? _config.DefaultExpirationInSeconds)
            };
            _memoryCache.Set(cacheKey, value, memoryCacheOptions);
        }
    }
}