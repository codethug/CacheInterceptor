using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace CacheInterceptor
{
    public static class ServicesExtensions
    {
        public static void AddProxiedScoped<TInterface, TImplementation>
            (this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            // This registers the underlying class
            services.AddScoped<TImplementation>();
            services.AddScoped(typeof(TInterface), serviceProvider =>
            {
                // Get an instance of the Castle Proxy Generator
                var proxyGenerator = serviceProvider
                    .GetRequiredService<ProxyGenerator>();

                // Have DI build out an instance of the class that has methods
                // you want to cache (this is a normal instance of that class 
                // without caching added)
                var actual = serviceProvider
                    .GetRequiredService<TImplementation>();

                // Find all of the interceptors that have been registered, 
                // including our caching interceptor.  (you might later add a 
                // logging interceptor, etc.)
                var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();

                // Have Castle Proxy build out a proxy object that implements 
                // your interface, but adds a caching layer on top of the
                // actual implementation of the class.  This proxy object is
                // what will then get injected into the class that has a 
                // dependency on TInterface
                return proxyGenerator.CreateInterfaceProxyWithTarget(
                    typeof(TInterface), actual, interceptors);
            });
        }
    }
}
