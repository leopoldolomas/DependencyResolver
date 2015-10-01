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

        private List<System.Type> interfacesToBeResolved;
        private List<System.Type> classesThatCanResolveDependencies;

        public DependencyResolver(string _namespace)
        {
            Logger = new DummyLogger();
            populateDependenciesMap(_namespace);
        }

        public DependencyResolver(string _namespace, ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger", "logger can't be NULL");
            }

            Logger = logger;
            populateDependenciesMap(_namespace);
        }

        private void populateDependenciesMap(string _namespace)
        {
            Logger.log("===================================================");
            // get the class list from the specified namespace
            var fullClassList = AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                t => t.GetTypes()).Where(t => (t.IsClass || t.IsInterface) && t.Namespace == @_namespace).ToList();

            // now we need to get all the interfaces that will need to be resolved
            interfacesToBeResolved = fullClassList.SelectMany(
                c => c.GetProperties()).Where(
                p => p.IsDefined(typeof(AutoResolvedAttribute), false) && p.PropertyType.IsInterface).Select(
                p => p.PropertyType).ToList();

            // if a class is intended to resolve a dependency, it should be marked with the "ResolvesDependency" attribute
            classesThatCanResolveDependencies = fullClassList.Where(c => c.IsDefined(typeof(ResolvesDependencyAttribute), false)).ToList();

            // TODO explain why we select interface #0
            var interfacesThatCanTrulyBeResolved = classesThatCanResolveDependencies.Select(c => c.GetInterfaces()[0]).Distinct().ToList();

            Logger.log("INTERFACE - ENUM_TYPE - ENUM_VALUE - CLASS");

            dependencyInfoList = new List<DependencyInfo>();
            foreach (var _class in classesThatCanResolveDependencies)
            {
                var attribute = _class.GetCustomAttributes(false).Where(a => a is ResolvesDependencyAttribute).First() as ResolvesDependencyAttribute;
                var _interface = _class.GetInterfaces().First(); // TODO needs to be tested thoroughly
                var dependencyInfo = new DependencyInfo(_interface, _class, attribute.EnumType, attribute.Value);

                bool configInUse = dependencyInfoList.Where(
                    d => d.InterfaceType == _interface && d.EnumType == attribute.EnumType && d.EnumValue == attribute.Value).Count() > 0;

                if (configInUse)
                {
                    var enumType = attribute.EnumType != null ? attribute.EnumType.Name : "null";
                    throw new ConfigurationAlreadyInUseException(
                        $"The following configuration tuple is already in use: {_interface.Name} - {enumType} - {attribute.Value}");
                }

                dependencyInfoList.Add(dependencyInfo);
            }

            dependencyInfoList.ForEach(
                d => Logger.log(
                    d.InterfaceType.Name + " - " + 
                    (d.EnumType != null ? d.EnumType.Name : "N/A") + " - " + 
                    d.EnumValue + " - " + 
                    d.ClassType.Name));
            Logger.log("===================================================");
        }

        public void ResolveDependencies(object obj, bool recursive)
        {
            // search for all properties with an interface as a type
            var propertyInfoList = obj.GetType().GetProperties().Where(
                t => interfacesToBeResolved.Contains(t.PropertyType)).ToList();

            foreach (var propertyInfo in propertyInfoList)
            {
                var interfaceType = propertyInfo.PropertyType;
                var dependencyInfo = dependencyInfoList.FirstOrDefault(d => d.InterfaceType == propertyInfo.PropertyType);

                if (dependencyInfo == null)
                {
                    throw new ClassNotFoundException($"Could not find a class to resolve the dependency: {interfaceType.Name}");
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
                var classType = dependencyInfoList.First(
                    d => d.InterfaceType == interfaceType && d.EnumType == enumType && d.EnumValue == enumValue).ClassType;

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

        public void ResolveDependencies(object obj)
        {
            ResolveDependencies(obj, false);
        }
    }
}
