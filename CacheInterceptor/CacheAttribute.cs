using System;

namespace CacheInterceptor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CacheAttribute : Attribute
    {
        public int? Seconds { get; private set; }

        public CacheAttribute()
        {
            Seconds = null;
        }

        public CacheAttribute(int Seconds)
        {
            this.Seconds = Seconds;
        }
    }
}
