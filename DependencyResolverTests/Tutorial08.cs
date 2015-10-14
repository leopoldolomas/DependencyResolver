using Leos.DependencyResolver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DependencyResolverTests
{
    [TestClass]
    public class Tutorial08
    {
        public enum AnimalType { Dog = 0, Cat = 1, Rabbit = 2 };

        public class Dog : IAnimal
        {
            public void MakeSound()
            {
                Console.WriteLine("woof woof!");
            }
        }

        public class Cat : IAnimal
        {
            public void MakeSound()
            {
                Console.WriteLine("meow!");
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
        public void TestMethod8()
        {
            var dependencyResolver = new Leos.DependencyResolver.DependencyResolver(new Logger());

            dependencyResolver.Bind<IAnimal>().To<Dog>().When<AnimalType>().IsEqualTo(AnimalType.Dog);
            dependencyResolver.Bind<IAnimal>().To<Cat>().When<AnimalType>().IsEqualTo(AnimalType.Cat);

            var animalCage = new AnimalCage();

            animalCage.AnimalType = AnimalType.Dog;
            dependencyResolver.ResolveDependencies(animalCage);
            Assert.IsTrue(animalCage.Animal is Dog);
            animalCage.GreetAnimal();

            animalCage.AnimalType = AnimalType.Cat;
            dependencyResolver.ResolveDependencies(animalCage);
            Assert.IsTrue(animalCage.Animal is Cat);
            animalCage.GreetAnimal();

            animalCage.AnimalType = AnimalType.Rabbit;

            try
            {
                dependencyResolver.ResolveDependencies(animalCage);
            }
            catch (ClassNotFoundException)
            {
                // correct! There is no binding for IAnimal when 'AnimalType = Rabbit'
            }
        }
    }
}
