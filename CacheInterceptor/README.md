This library is designed to help a .Net Core project add Caching using AOP (Aspect Oriented Programming).  To do this, we intercept calls to methods, do a lookup in the cache to see if the method has been called with the parameter values given, and return the cached values if they exist.

The only change required to the method being cached is to add the `[Cache]` attribute:

``` c#
[Cache]
public Customer[] GetCustomers(string zipCode)
{
  var url = $"Customers/ByZipCode/{zipCode}";
  return await _customersApi.GetAsync<Customer[]>>(url);
}
```

This is compatible with projects running .Net Core 3.1 or later.


## Visual Studio Project Configuration
This library comes with one options for caching - an in-memory cache.  Other caching implementations can be built by implementing `CacheInterceptor<CacheConfig>`.

This library requires configuration.  Add a config section like this to appsettings.json:

For In-Memory Caching:
``` js
"CacheInterceptor": {
  "CacheKeyPrefix": "Dev",
  "DefaultExpirationInSeconds": 90,
},
```

## Startup.cs

We need to setup interception in startup.cs at the top of the `ConfigureServices` method:

For In-Memory caching:
``` c#
// using Castle.DynamicProxy;
// using CacheInterceptor.InMemory;
services.AddSingleton(Configuration.GetSection("CacheInterceptor").Get<MemoryCacheConfig>());
services.AddSingleton(new ProxyGenerator());
services.AddScoped<IAsyncInterceptor, MemoryCacheInterceptor>();
```


Later on in `ConfigureServices`, setup DI for the class with methods that need to be cached like this:

``` c#
// using CacheInterceptor;
services.AddProxiedScoped<ICustomerService, CustomerService>();
```

Note that you must have an interface for the class, and the interface must be injected into the calling class in order for this to work.


## Add caching to a method

To add caching to a method - so that the result calls to a method are cached - add the cache attribute.  The settings in `appsettings.json` define the default expiration in seconds, which can be overridden in the `Cache` attribute like this:

``` c#
[Cache(Seconds: 20)]
public Customer[] GetCustomers(string zipCode)
{
  var url = $"Customers/ByZipCode/{zipCode}";
  return await _customersApi.GetAsync<Customer[]>>(url);
}
```