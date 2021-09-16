using Castle.DynamicProxy;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CacheInterceptor
{
    // Built with help from https://stackoverflow.com/questions/28099669/intercept-async-method-that-returns-generic-task-via-dynamicproxy
    // and https://github.com/castleproject/Core/blob/master/docs/dynamicproxy-async-interception.md

    public abstract class CacheInterceptor<TConfig> : IAsyncInterceptor where TConfig : CacheConfig
    {
        protected TConfig _config;
        public CacheInterceptor(TConfig config)
        {
            _config = config;
        }

        public abstract bool TryGetCacheValue<TResult>(string cacheKey, out TResult cacheValue);

        public abstract void SetCacheValue<TItem>(string cacheKey, TItem value, CacheAttribute options);

        // Create a cache key using the name of the method and the values
        // of its arguments so that if the same method is called with the
        // same arguments in the future, we can find out if the results 
        // are cached or not
        private string GenerateCacheKey(string name,
            object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
                return name;
            return _config.CacheKeyPrefix + "--" + name + "--" +
                string.Join("--", arguments.Select(a =>
                    a == null ? "**NULL**" : a.ToString()).ToArray());
        }

        private static readonly MethodInfo internalInterceptSynchronousMethodInfo = typeof(CacheInterceptor<TConfig>).GetMethod("InternalInterceptSynchronous", BindingFlags.Instance | BindingFlags.NonPublic);

        public void InterceptSynchronous(IInvocation invocation)
        {
            var cacheAttribute = invocation.MethodInvocationTarget
                .GetCustomAttributes(typeof(CacheAttribute), false)
                .FirstOrDefault() as CacheAttribute;

            if (_config.CacheKeyPrefix != null && cacheAttribute != null)
            {
                // Using reflection so that TryGetCacheValue and SetCacheValue can be
                // generic, making these classes easier to use
                var internalInterceptSynchronousMethod = internalInterceptSynchronousMethodInfo.MakeGenericMethod(invocation.Method.ReturnType);
                var internalInterceptSynchronousParams = new object[] { invocation, cacheAttribute };

                internalInterceptSynchronousMethod.Invoke(this, internalInterceptSynchronousParams);
            }
            else
            {
                // Caching is not available
                invocation.Proceed();
            }
        }

        private void InternalInterceptSynchronous<TResult>(IInvocation invocation, CacheAttribute options)
        {
            var cacheKey = GenerateCacheKey(invocation.Method.Name, invocation.Arguments);
            if (TryGetCacheValue(cacheKey, out TResult cacheValue))
            {
                // The results were already in the cache so return 
                // them from the cache instead of calling the underlying method
                invocation.ReturnValue = cacheValue;
            }
            else
            {
                // Get the result the slow way by calling the underlying method
                invocation.Proceed();

                // Once the real result is returned, save it in the cache
                SetCacheValue(cacheKey, invocation.ReturnValue, options);
            }
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
        }

        private async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            // There is no return value, so the cache is irrelevant
            invocation.Proceed();
            await (Task)invocation.ReturnValue;
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var cacheAttribute = invocation.MethodInvocationTarget
                .GetCustomAttributes(typeof(CacheAttribute), false)
                .FirstOrDefault() as CacheAttribute;

            if (_config.CacheKeyPrefix != null && cacheAttribute != null)
            {
                invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation, cacheAttribute);
            }
            else
            {
                // We don't need to cache the results, nothing to see here
                invocation.Proceed();
            }
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation, CacheAttribute options)
        {
            var cacheKey = GenerateCacheKey(invocation.Method.Name, invocation.Arguments);
            if (!TryGetCacheValue(cacheKey, out TResult result))
            {
                // We couldn't find the result in the cache, so 
                // get the result the slow way by calling the underlying method
                invocation.Proceed();
                result = await (Task<TResult>)invocation.ReturnValue;

                // Once the real result is found, save it in the cache
                SetCacheValue(cacheKey, result, options);
            }
            return result;
        }
    }
}