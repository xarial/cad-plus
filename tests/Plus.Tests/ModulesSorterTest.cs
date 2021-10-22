using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Tests
{
    public class ModuleTypeComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if ((y as Type).IsAssignableFrom(x as Type))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }

    public class ModulesSorterTest
    {
        [ModuleOrder(1)]
        public interface M1 : IModule
        {
        }

        [ModuleOrder(3)]
        public interface M2 : IModule
        {
        }

        [ModuleOrder(-1)]
        public interface M3 : IModule
        {
        }

        [ModuleOrder(-2)]
        public interface M4 : IModule
        {
        }

        [ModuleOrder(0)]
        public interface M5 : IModule
        {
        }

        public interface M6 : IModule
        {
        }

        [ModuleOrder(typeof(M6), ModuleRelativeOrder_e.After)]
        public interface M7 : IModule
        {
        }

        [ModuleOrder(typeof(B1), ModuleRelativeOrder_e.Before)]
        public interface M8 : IModule
        {
        }

        public interface B1 : IModule 
        {
        }

        public interface M9 : B1
        {
        }

        public interface M10 : IModule 
        {
        }

        public interface M11 : IModule
        {
        }

        public interface M12 : IModule
        {
        }

        public interface M13 : IModule
        {
        }

        [ModuleOrder(typeof(M13), ModuleRelativeOrder_e.After)]
        public interface M14 : IModule
        {
        }

        [ModuleOrder(typeof(M14), ModuleRelativeOrder_e.After)]
        public interface M15 : IModule
        {
        }

        [ModuleOrder(typeof(M17), ModuleRelativeOrder_e.After)]
        public interface M16 : IModule
        {
        }

        [ModuleOrder(typeof(M16), ModuleRelativeOrder_e.After)]
        public interface M17 : IModule
        {
        }

        [Test]
        public void TestUndefinedOrder() 
        {
            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[] 
            {
                Substitute.For<M10>(),
                Substitute.For<M11>(),
                Substitute.For<M12>()
            });

            CollectionAssert.AreEqual(res.Select(x => x.GetType()), 
                new Type[] 
                {
                    typeof(M10),
                    typeof(M11),
                    typeof(M12)
                }, new ModuleTypeComparer());
        }

        [Test]
        public void TestDefinedOrder()
        {
            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[] 
            {
                Substitute.For<M2>(), 
                Substitute.For<M12>(), 
                Substitute.For<M1>(),
                Substitute.For<M5>(),
                Substitute.For<M3>(),
                Substitute.For<M4>()
            });

            CollectionAssert.AreEqual(res.Select(x => x.GetType()),
                new Type[]
                {
                    typeof(M4),
                    typeof(M3),
                    typeof(M12),
                    typeof(M5),
                    typeof(M1),
                    typeof(M2)
                }, new ModuleTypeComparer());
        }

        [Test]
        public void TestDependency()
        {
            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[] 
            {
                Substitute.For<M7>(),
                Substitute.For<M9>(),
                Substitute.For<M6>(),
                Substitute.For<M8>()
            });

            CollectionAssert.AreEqual(res.Select(x => x.GetType()),
                new Type[]
                {
                    typeof(M8),
                    typeof(M9),
                    typeof(M6),
                    typeof(M7)
                }, new ModuleTypeComparer());
        }

        [Test]
        public void TestDoubleDependency()
        {
            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[]
            {
                Substitute.For<M15>(),
                Substitute.For<M14>(),
                Substitute.For<M13>()
            });

            CollectionAssert.AreEqual(res.Select(x => x.GetType()),
                new Type[]
                {
                    typeof(M13),
                    typeof(M14),
                    typeof(M15)
                }, new ModuleTypeComparer());
        }

        [Test]
        public void TestMissingDependency()
        {
            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[]
            {
                Substitute.For<M15>(),
                Substitute.For<M14>(),
                Substitute.For<M12>()
            });

            CollectionAssert.AreEqual(res.Select(x => x.GetType()),
                new Type[]
                {
                    typeof(M12),
                    typeof(M14),
                    typeof(M15)
                }, new ModuleTypeComparer());
        }

        [Test]
        public void TestCircularDependency()
        {
            var sorter = new ModulesSorter();

            Assert.Throws<ModuleOrderCircularDependencyException>(() => sorter.Sort(new IModule[]
            {
                Substitute.For<M16>(),
                Substitute.For<M17>()
            }));
        }
    }
}
