using Castle.DynamicProxy;
using EPiServer.ServiceLocation;
using ESerializer.Utils;
using JsonContractSimplifier;
using JsonContractSimplifier.Services.ConverterLocator;
using System;
using System.Linq;

namespace ESerializer.Loader
{
    [ServiceConfiguration(typeof(IConverterLocatorService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ConverterLoader : IConverterLocatorService
    {
        private readonly DeriveTypeCompare _deriveTypeComparer = new DeriveTypeCompare();

        private (IConverter Converter, Type Target)[] _converters;       
        private (IConverter Converter, Type Target)[] Converters { get
            {
                if(_converters == null)
                {
                    _converters = LoadConverters();
                }

                return _converters;
            }
        }

        private static string LocalAssemblyName =>
            string.Join(".", typeof(ConverterLoader).Assembly.GetName().Name.Split('.').Take(2));

        private static bool IsLocalAssemblyConverter(Type type) => type.Assembly.FullName.Contains(LocalAssemblyName);        
        
        private (IConverter Converter, Type Target)[] LoadConverters()
        {
            var allConverters = ServiceLocator.Current.GetAllInstances<IConverter>().ToArray();

            var localAssemblyConverters = allConverters
                .Where(x => IsLocalAssemblyConverter(x.GetType()));

            var otherAssemblyConverters = allConverters
                .Where(x => IsLocalAssemblyConverter(x.GetType()) == false);

            return otherAssemblyConverters
                .Concat(
                    localAssemblyConverters
                    .Where(x => otherAssemblyConverters.Any(y => y.Equals(x)) == false)
                )
                .Select(converter =>
                    (converter, converter.GetType().GetInterfaces()[0].GetGenericArguments()[0])
                )
                .ToArray();
        }

        public virtual object Convert(object target, IConverter converter)
        {
            Type converterType = converter.GetType();

            if (!converterType.ImplementsGenericInterface(typeof(IObjectConverter<>)))
            {
                throw new ArgumentException($"Converter type {converterType.Name} does not implement IObjectConverter");
            }

            var method = converterType.GetMethod("Convert");
            return method.Invoke(converter, new[] { target });
        }
        
        public virtual bool TryFindConverterFor(Type type, out IConverter converter)
        {
            Type compareType = typeof(IProxyTargetAccessor).IsAssignableFrom(type) ? type.BaseType : type;
            
            bool typeIsEqual((IConverter Converter, Type Target) tuple)
                => tuple.Target == compareType || compareType.IsSubclassOf(tuple.Target);
            
            var converters = Converters.Where(typeIsEqual).OrderBy(x => x.Target, _deriveTypeComparer);

            bool exist = converters.Any();
            
            if (!exist)
            {
                converter = null;
                return false;
            }

            converter = converters.First().Converter;
            return true;
        }        
    }
}
