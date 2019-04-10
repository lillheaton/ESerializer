using Castle.DynamicProxy;
using JsonContractSimplifier.Services.ConverterLocator;
using JsonContractSimplifier.Services.Reflection;
using System;
using System.Linq;

namespace ESerializer.Loader
{
    public class ConverterLoader : ConverterLocatorService
    {
        private static string LocalAssemblyName =>
            string.Join(".", typeof(ConverterLoader).Assembly.GetName().Name.Split('.').Take(2));

        private static bool IsLocalAssemblyConverter(Type type) => type.Assembly.FullName.Contains(LocalAssemblyName);

        public override Type[] LoadConverters()
        {
            var allConverters = ReflectionService
                .GetAssemblyClassesInheritGenericInterface(
                    typeof(IApiPropertyConverter<>), 
                    AppDomain.CurrentDomain.GetAssemblies()
                );

            var localAssemblyConverters = allConverters.Where(IsLocalAssemblyConverter);
            var otherAssemblyConverters = allConverters.Where(x => IsLocalAssemblyConverter(x) == false);

            return otherAssemblyConverters
                .Concat(
                    localAssemblyConverters
                    .Where(x => otherAssemblyConverters.Any(y => y.Equals(x)) == false)
                ).ToArray();
        }      

        public override bool TryFindConverterFor(Type type, out IConverter converter)
        {
            bool typeIsEqual ((IConverter Converter, Type Target) tuple) 
                => tuple.Target == type
                || (typeof(IProxyTargetAccessor).IsAssignableFrom(type) ? tuple.Target == type.BaseType : false);
            
            bool exist = _converters.Any(typeIsEqual);

            if (!exist)
            {
                converter = null;
                return false;
            }

            converter = _converters.First(typeIsEqual).Converter;
            return true;
        }
    }
}
