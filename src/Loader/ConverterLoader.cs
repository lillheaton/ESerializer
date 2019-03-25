using EOls.Serialization.Services.ConverterLocator;
using EOls.Serialization.Services.Reflection;
using System;
using System.Linq;

namespace EPiSerializer.Loader
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
    }
}
