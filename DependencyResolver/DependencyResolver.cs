//--------------------------------------------------------------- @License begins
// "DependencyResolver"
// 2015 Leopoldo Lomas Flores. Torreon, Coahuila. MEXICO
// leopoldolomas [at] gmail
// www.leopoldolomas.info
// 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or distribute this
// software, either in source code form or as a compiled binary, for any purpose,
// commercial or non-commercial, and by any means.
// 
// In jurisdictions that recognize copyright laws, the author or authors of this
// software dedicate any and all copyright interest in the software to the public
// domain. We make this dedication for the benefit of the public at large and to
// the detriment of our heirs and successors. We intend this dedication to be
// an overt act of relinquishment in perpetuity of all present and future
// rights to this software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//--------------------------------------------------------------- @License ends

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Leos.DependencyResolver
{
    public class DependencyResolver
    {
        public ILogger Logger { get; set; }

        private List<DependencyInfo> dependencyInfoList;

        public DependencyResolver(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger", "logger can't be NULL");
            }

            Logger = logger;

            dependencyInfoList = new List<DependencyInfo>();
        }

        public DependencyResolver(string _namespace)
        {
            Logger = new DummyLogger();
            populateDependenciesMap(_namespace);
        }

        public DependencyResolver(string _namespace, ILogger logger) : this(logger)
        {
            populateDependenciesMap(_namespace);
        }

        public void Bind(Type interface_, Type class_)
        {
            Bind(interface_, class_, null, 0);
        }

        public Bind<T> Bind<T>()
        {
            dependencyInfoList.Add(new DependencyInfo());
            var dependencyInfo = dependencyInfoList.Last();
            var bind = new Bind<T>();
            bind.DependencyInfo = dependencyInfo;

            return bind;
        }

        public void Bind(Type dependencyType, Type serviceType, Type enumType, int value)
        {
            if (dependencyType == null)
            {
                throw new ArgumentNullException("dependencyType");
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            var dependencyInfo = new DependencyInfo(dependencyType, serviceType, enumType, value);

            bool configInUse = dependencyInfoList.Where(
                d => d.DependencyType == dependencyType && d.EnumType == enumType && d.EnumValue == value).Count() > 0;

            if (configInUse)
            {
                throw new ConfigurationAlreadyInUseException(
                    $"The following configuration tuple is already in use: {dependencyType.Name} - {enumType.Name} - {Enum.GetName(enumType, value)}");
            }

            dependencyInfoList.Add(dependencyInfo);
        }

        private void populateDependenciesMap(string _namespace)
        {
            Logger.log("===================================================");

            // get the class list from the specified namespace
            var fullClassList = AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                t => t.GetTypes()).Where(t => (t.IsClass || t.IsInterface) && t.Namespace == @_namespace).ToList();

            // if a class is intended to resolve a dependency, it should have been marked with the [ResolvesDependency] attribute
            var availableServices = fullClassList.Where(c => c.IsDefined(typeof(ResolvesDependencyAttribute), false)).ToList();

            Logger.log("DEPENDENCY_TYPE - ENUM_TYPE - ENUM_VALUE - SERVICE_TYPE");

            dependencyInfoList = new List<DependencyInfo>();
            foreach (var _class in availableServices)
            {
                var attribute = _class.GetCustomAttributes(false).Where(a => a is ResolvesDependencyAttribute).First() as ResolvesDependencyAttribute;
                var _interface = _class.GetInterfaces().First(); // TODO needs to be tested thoroughly

                Bind(_interface, _class, attribute.EnumType, attribute.Value);
            }

            dependencyInfoList.ForEach(
                d => Logger.log(
                    $"{d.DependencyType.Name} - {(d.EnumType != null ? d.EnumType.Name : "N/A")} - {(d.EnumType != null ? Enum.GetName(d.EnumType, d.EnumValue) : "N/A")} - {d.ServiceType.Name}"));
            Logger.log("===================================================");
        }

        public void ResolveDependencies(object obj)
        {
            ResolveDependencies(obj, false);
        }

        public void ResolveDependencies(object obj, bool recursive)
        {
            var propertyInfoList = obj.GetType().GetProperties().Where(t => t.IsDefined(typeof(AutoResolvedAttribute))).ToList();

            foreach (var propertyInfo in propertyInfoList)
            {
                var dependencyType = propertyInfo.PropertyType;
                var dependencyInfo = dependencyInfoList.FirstOrDefault(d => d.DependencyType == propertyInfo.PropertyType);

                if (dependencyInfo == null)
                {
                    throw new ClassNotFoundException($"Could not find a service to resolve the following dependency: {dependencyType.Name}");
                }

                var enumType = dependencyInfo.EnumType;

                int enumValue = 0;
                if (enumType != null)
                {
                    // query the enum value
                    var enumProperty = obj.GetType().GetProperties().FirstOrDefault(
                        p => p.IsDefined(typeof(QueryableByDependencyResolver)) && p.PropertyType == enumType);

                    if (enumProperty == null)
                    {
                        throw new QueryablePropertyNotFoundException(
                            $"Expected queryable property of type [{dependencyInfo.EnumType.Name}] in class [{obj.GetType().Name}]");
                    }

                    enumValue = (int)enumProperty.GetValue(obj);
                }

                // determine which class must be used to resolve the dependency
                dependencyInfo = dependencyInfoList.FirstOrDefault(
                    d => d.DependencyType == dependencyType && d.EnumType == enumType && d.EnumValue == enumValue);

                if (dependencyInfo == null)
                {
                    throw new ClassNotFoundException($"Could not find a service to resolve the following " + 
                        $"dependency: {dependencyType.Name}. Configuration Tuple: {enumType.Name} - {Enum.GetName(enumType, enumValue)}");
                }

                var classType = dependencyInfo.ServiceType;

                // should inject the dependency only if the current property is null or has a different Type
                if (propertyInfo.GetValue(obj) == null || propertyInfo.GetValue(obj).GetType() != classType)
                {
                    var dependency = Activator.CreateInstance(classType);
                    propertyInfo.SetValue(obj, dependency);

                    if (recursive)
                    {
                        ResolveDependencies(dependency, recursive);
                    }
                }
            }
        }
    }
}
