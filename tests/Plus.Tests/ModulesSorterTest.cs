using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Atributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Tests
{
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

        [ModuleOrder("D2275851-89C1-490D-9BDD-7B69A3EC2982", ModuleRelativeOrder_e.After)]
        public interface M6 : IModule
        {
        }

        [ModuleOrder("C2D21F8E-E23E-42AB-AC21-F6E07783E886", ModuleRelativeOrder_e.After)]
        public interface M7 : IModule
        {
        }

        [ModuleOrder("D2275851-89C1-490D-9BDD-7B69A3EC2982", ModuleRelativeOrder_e.Before)]
        public interface M8 : IModule
        {
        }

        [ModuleOrder("C2D21F8E-E23E-42AB-AC21-F6E07783E886", ModuleRelativeOrder_e.Before)]
        public interface M9 : IModule
        {
        }

        [Test]
        public void TestUndefinedOrder() 
        {
            var m1 = Substitute.For<IModule>();
            m1.Id.Returns(Guid.Parse("D2275851-89C1-490D-9BDD-7B69A3EC2982"));

            var m2 = Substitute.For<IModule>();
            m2.Id.Returns(Guid.Parse("C2D21F8E-E23E-42AB-AC21-F6E07783E886"));

            var m3 = Substitute.For<IModule>();
            m3.Id.Returns(Guid.Parse("5AEE3C66-89C3-4944-95FA-351AD52C48FD"));

            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[] { m1, m2, m3 });

            CollectionAssert.AreEqual(res.Select(x => x.Id.ToString()), 
                new string[] 
                {
                    "D2275851-89C1-490D-9BDD-7B69A3EC2982", 
                    "C2D21F8E-E23E-42AB-AC21-F6E07783E886",
                    "5AEE3C66-89C3-4944-95FA-351AD52C48FD"
                }, StringComparer.CurrentCultureIgnoreCase);
        }

        [Test]
        public void TestDefinedOrder() 
        {
            var m1 = Substitute.For<M2>();
            m1.Id.Returns(Guid.Parse("D2275851-89C1-490D-9BDD-7B69A3EC2982"));

            var m2 = Substitute.For<M1>();
            m2.Id.Returns(Guid.Parse("C2D21F8E-E23E-42AB-AC21-F6E07783E886"));

            var m3 = Substitute.For<M5>();
            m3.Id.Returns(Guid.Parse("5AEE3C66-89C3-4944-95FA-351AD52C48FD"));

            var m4 = Substitute.For<M3>();
            m4.Id.Returns(Guid.Parse("3F2B2942-A30E-4283-9146-8EBA50D9976D"));

            var m5 = Substitute.For<M4>();
            m5.Id.Returns(Guid.Parse("FAF9A11C-066F-4946-A4A3-EB39C0E0720B"));

            var m6 = Substitute.For<IModule>();
            m6.Id.Returns(Guid.Parse("DA786768-B76E-4A67-BA34-FA99BE3AB1FC"));

            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[] { m1, m2, m3, m4, m5, m6 });

            CollectionAssert.AreEqual(res.Select(x => x.Id.ToString()),
                new string[]
                {
                    "FAF9A11C-066F-4946-A4A3-EB39C0E0720B",
                    "3F2B2942-A30E-4283-9146-8EBA50D9976D",
                    "5AEE3C66-89C3-4944-95FA-351AD52C48FD",
                    "DA786768-B76E-4A67-BA34-FA99BE3AB1FC",
                    "C2D21F8E-E23E-42AB-AC21-F6E07783E886",
                    "D2275851-89C1-490D-9BDD-7B69A3EC2982"
                }, StringComparer.CurrentCultureIgnoreCase);
        }

        [Test]
        public void TestDependency() 
        {
            var m1 = Substitute.For<IModule>();
            m1.Id.Returns(Guid.Parse("D2275851-89C1-490D-9BDD-7B69A3EC2982"));

            var m2 = Substitute.For<IModule>();
            m2.Id.Returns(Guid.Parse("DA786768-B76E-4A67-BA34-FA99BE3AB1FC"));

            var m3 = Substitute.For<IModule>();
            m3.Id.Returns(Guid.Parse("C2D21F8E-E23E-42AB-AC21-F6E07783E886"));

            var m4 = Substitute.For<M6>();
            m4.Id.Returns(Guid.Parse("A5936FE3-8271-490D-B15D-A05DAE661C12"));

            var m5 = Substitute.For<M7>();
            m5.Id.Returns(Guid.Parse("2DF8FEB7-2580-43B3-A61A-460A0A3424D3"));

            var m6 = Substitute.For<M8>();
            m6.Id.Returns(Guid.Parse("FDED7FA9-75ED-4D11-934D-25B5A21304DA"));

            var m7 = Substitute.For<M9>();
            m7.Id.Returns(Guid.Parse("974DAF9E-DDEE-464D-8E4A-9A9BF0BF1DF4"));

            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[] { m1, m2, m3, m4, m5, m6, m7 });

            CollectionAssert.AreEqual(res.Select(x => x.Id.ToString()),
                new string[]
                {
                    "fded7fa9-75ed-4d11-934d-25b5a21304da",
                    "974daf9e-ddee-464d-8e4a-9a9bf0bf1df4",
                    "d2275851-89c1-490d-9bdd-7b69a3ec2982",
                    "da786768-b76e-4a67-ba34-fa99be3ab1fc",
                    "c2d21f8e-e23e-42ab-ac21-f6e07783e886",
                    "a5936fe3-8271-490d-b15d-a05dae661c12",
                    "2df8feb7-2580-43b3-a61a-460a0a3424d3"
                }, StringComparer.CurrentCultureIgnoreCase);
        }

        [Test]
        public void TestDoubleDependency()
        {
            var m1 = Substitute.For<IModule>();
            m1.Id.Returns(Guid.Parse("C2D21F8E-E23E-42AB-AC21-F6E07783E886"));

            var m2 = Substitute.For<M6>();
            m2.Id.Returns(Guid.Parse("B1FFD24A-C6F1-42EF-8B5B-89F25875BF67"));

            var m3 = Substitute.For<M9>();
            m3.Id.Returns(Guid.Parse("D2275851-89C1-490D-9BDD-7B69A3EC2982"));
            
            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[] { m1, m2, m3 });

            CollectionAssert.AreEqual(res.Select(x => x.Id.ToString()),
                new string[]
                {
                    "D2275851-89C1-490D-9BDD-7B69A3EC2982",
                    "C2D21F8E-E23E-42AB-AC21-F6E07783E886",
                    "B1FFD24A-C6F1-42EF-8B5B-89F25875BF67"
                }, StringComparer.CurrentCultureIgnoreCase);
        }

        [Test]
        public void TestMissingDependency()
        {
            var m1 = Substitute.For<IModule>();
            m1.Id.Returns(Guid.Parse("29FECDE6-5C45-42EF-B67E-E409453B607E"));

            var m2 = Substitute.For<M8>();
            m2.Id.Returns(Guid.Parse("B1FFD24A-C6F1-42EF-8B5B-89F25875BF67"));

            var m3 = Substitute.For<M9>();
            m3.Id.Returns(Guid.Parse("D2275851-89C1-490D-9BDD-7B69A3EC2982"));

            var sorter = new ModulesSorter();
            var res = sorter.Sort(new IModule[] { m1, m2, m3 });

            CollectionAssert.AreEqual(res.Select(x => x.Id.ToString()),
                new string[]
                {
                    "B1FFD24A-C6F1-42EF-8B5B-89F25875BF67",
                    "29FECDE6-5C45-42EF-B67E-E409453B607E",
                    "D2275851-89C1-490D-9BDD-7B69A3EC2982"
                }, StringComparer.CurrentCultureIgnoreCase);
        }

        [Test]
        public void TestCircularDependency()
        {
            var m1 = Substitute.For<M7>();
            m1.Id.Returns(Guid.Parse("D2275851-89C1-490D-9BDD-7B69A3EC2982"));

            var m2 = Substitute.For<M6>();
            m2.Id.Returns(Guid.Parse("C2D21F8E-E23E-42AB-AC21-F6E07783E886"));

            var sorter = new ModulesSorter();
            Assert.Throws<ModuleOrderCircularDependencyException>(() => sorter.Sort(new IModule[] { m1, m2 }));
        }
    }
}
