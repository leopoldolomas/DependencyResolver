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

namespace DependencyResolverTests_Tutorial07
{
    [TestClass]
    public class Tutorial07
    {
        public class Dog : IAnimal
        {
            public void MakeSound()
            {
                Console.WriteLine("woof woof!");
            }
        }

        public class AnimalCage
        {
            public AnimalCage()
            {
                Animal = null;
            }

            // even though we are using manual binding this time, we still need to tell DependecyResolver
            // what dependencies need to be resolved
            [AutoResolved]
            public IAnimal Animal { get; set; }

            // since this property does not make use of the [AutoResolved] attribute, it should not be resolved
            public IAnimal Animal2 { get; set; }

            public void GreetAnimal()
            {
                Animal.MakeSound();
                Animal2.MakeSound();
            }
        }

        [TestMethod]
        public void TestMethod7()
        {
            var dependencyResolver = new Leos.DependencyResolver.DependencyResolver(new Logger());
            dependencyResolver.Bind<IAnimal>().To<Dog>(); // manual binding
            var animalCage = new AnimalCage();
            dependencyResolver.ResolveDependencies(animalCage);

            Assert.IsNotNull(animalCage.Animal, "Animal1 dependency could not be resolved");
            Assert.IsNull(animalCage.Animal2, "Animal2 should not have been resolved");
        }
    }
}
