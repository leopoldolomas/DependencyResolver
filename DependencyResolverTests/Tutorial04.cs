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

using DependencyResolverTests;
using Leos.DependencyResolver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace DependencyResolverTests_Tutorial04
{
    [TestClass]
    public class Tutorial04
    {
        public enum AnimalType { Dog = 0, Cat = 1, Duck = 2 };

        [ResolvesDependency(typeof(AnimalType), (int)AnimalType.Dog)]
        public class Dog : IAnimal
        {
            public void MakeSound()
            {
                Console.WriteLine("woof woof!");
            }
        }

        [ResolvesDependency(typeof(AnimalType), (int)AnimalType.Cat)]
        public class Cat : IAnimal
        {
            public void MakeSound()
            {
                Console.WriteLine("meow!");
            }
        }

        // ERROR! AnimalType.Cat is already in use by 'Cat' class
        [ResolvesDependency(typeof(AnimalType), (int)AnimalType.Cat)]
        public class Duck : IAnimal
        {
            public void MakeSound()
            {
                Console.WriteLine("quack quack!");
            }
        }

        public class AnimalCage
        {
            [QueryableByDependencyResolver]
            public AnimalType AnimalType { get; set; }

            public AnimalCage()
            {
                Animal = null;
            }

            [AutoResolved]
            public IAnimal Animal { get; set; }

            public void GreetAnimal()
            {
                Animal.MakeSound();
            }
        }

        [TestMethod]
        public void TestMethod4()
        {
            try
            {
                var dependencyResolver = new Leos.DependencyResolver.DependencyResolver("DependencyResolverTests_Tutorial04", new Logger());
                Assert.Fail("Expected exception of type 'ConfigurationAlreadyInUseException'");
            }
            catch(ConfigurationAlreadyInUseException ex)
            {
                Trace.WriteLine(ex.Message);
            } 
            // we don't even need to test anything else, DependencyResolver must crash when populating the DependencyInfo list
        }
    }
}
