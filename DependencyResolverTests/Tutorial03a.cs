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

namespace DependencyResolverTests_Tutorial03a
{
    [TestClass]
    public class Tutorial03a
    {
        interface IWeapon
        {
            int GetHitDamage();
        }

        interface IPlayer
        {
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
            [AutoResolved]
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
        public void TestMethod3a()
        {
            var dependencyResolver = new Leos.DependencyResolver.DependencyResolver("DependencyResolverTests_Tutorial03a", new Logger());
            var game = new Game();
            dependencyResolver.ResolveDependencies(game, recursive: true);

            Assert.IsTrue(game.Player is Ninja);
            Assert.IsTrue(game.Player.Weapon is Sword);

            game.Player.HitEnemy();
        }
    }
}
            

