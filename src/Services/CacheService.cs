using JsonContractSimplifier.Services.Cache;
using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;
using System;

namespace ESerializer.Services
{
    [ServiceConfiguration(typeof(ICacheService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CacheService : ICacheService
    {
        private readonly ISynchronizedObjectInstanceCache _cache;

        public CacheService(ISynchronizedObjectInstanceCache synchronizedObjectInstanceCache)
        {
            _cache = synchronizedObjectInstanceCache;
        }
        public CacheService() : this(
            ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>())
        {
        }

        public T HandleCache<T>(string key, Func<T> func)
        {
            return HandleCache(key, TimeSpan.FromMinutes(10), func);
        }
        public T HandleCache<T>(string key, TimeSpan timeSpan, Func<T> func)
        {
            T target;
            if (TryGet(key, out target))
            {
                return target;
            }

            target = func();

            Set(key, target, timeSpan);
            return target;
        }

        public void Set(string key, object value, TimeSpan timeSpan)
        {
            _cache
                .Insert(
                    key,
                    value,
                    new CacheEvictionPolicy(
                        timeSpan,
                        CacheTimeoutType.Sliding
                    )
                );
        }

        public bool TryGet<T>(string key, out T value)
        {
            var target = _cache.Get(key);
            if (target != null)
            {
                value = (T)target;
                return true;
            }

            value = default(T);
            return false;
        }

        public void ClearAll()
        {
            throw new NotImplementedException();
        }
    }
}
