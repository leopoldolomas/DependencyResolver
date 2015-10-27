using DependencyResolverTests;
using Leos.DependencyResolver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DependencyResolverTests_Tutorial02a
{
    [TestClass]
    public class Tutorial02a
    {
        public enum AnimalType { Dog = 0, Cat = 1 };

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

        public class AnimalCage
        {
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
        public void TestMethod2a()
        {
            var dependencyResolver = new Leos.DependencyResolver.DependencyResolver("DependencyResolverTests_Tutorial02a", new Logger());
            var animalCage = new AnimalCage();

            try
            {
                dependencyResolver.ResolveDependencies(animalCage);
                Assert.Fail("Expected exception of type 'QueryablePropertyNotFoundException'");
            }
            catch (QueryablePropertyNotFoundException)
            {
                // correct! a queryable property of type AnimalType needs to be defined in class AnimalCage
            }
        }
    }
}
