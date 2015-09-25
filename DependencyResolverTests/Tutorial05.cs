using DependencyResolverTests;
using Leos.DependencyResolver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyResolverTests_Tutorial05
{
    // following is an example found in Ninject, here is how to implement it using DependencyResolver
    [TestClass]
    public class Tutorial05
    {
        interface IWeapon
        {
            int GetHitDamage();
        }

        interface IPlayer
        {
            [AutoResolved]
            IWeapon Weapon { get; set; }

            void HitEnemy();
        }

        [ResolvesDependency]
        class Sword : IWeapon
        {
            public int GetHitDamage()
            {
                return 5;
            }
        }

        [ResolvesDependency]
        class Ninja : IPlayer
        {
            public IWeapon Weapon { get; set; }

            public void HitEnemy()
            {
                Console.WriteLine($"Damage taken by the enemy using [{Weapon.GetType().Name}]: {Weapon.GetHitDamage()}");
            }
        }

        class Game
        {
            [AutoResolved]
            public IPlayer Player { get; set; }
        }

        [TestMethod]
        public void TestMethod5()
        {
            var dependencyResolver = new Leos.DependencyResolver.DependencyResolver("DependencyResolverTests_Tutorial05", new Logger());
            var game = new Game();
            dependencyResolver.ResolveDependencies(game, recursive: true);

            Assert.IsTrue(game.Player is Ninja);
            Assert.IsTrue(game.Player.Weapon is Sword);

            game.Player.HitEnemy();
        }
    }
}
            

