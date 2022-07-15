﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.DI;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Shared.DI;

namespace Shared.Tests
{
    public interface I1
    {
    }

    public class C1 : I1
    {
    }

    public interface I2 : I3
    {
    }

    public class C2 : I2
    {
    }

    public interface I3
    {
    }

    public interface I4
    {
        void Init();
    }

    public class C4 : I4
    {
        private I3 m_I3;

        public C4(I3 i3)
        {
            m_I3 = i3;
        }

        public void Init()
        {
        }
    }

    public interface I5
    {
    }

    public class C5 : I5
    {
        public C5()
        {
        }
    }

    public interface I6
    {
    }

    public class C6 : I6
    {
        private readonly I1 m_I1;
        private readonly I2 m_I2;
        private readonly string m_Test;

        public C6(I1 i1, I2 i2, string test)
        {
            m_I1 = i1;
            m_I2 = i2;
            m_Test = test;
        }
    }

    public class D2 : I2
    {
        private readonly I2 m_I2;

        public D2(I2 i2)
        {
            m_I2 = i2;
        }
    }

    public interface I7
    {
    }

    public class C7_1 : I7
    {
    }

    public class C7_2 : I7
    {
    }

    public class C7_3 : I7
    {
        public int Val { get; }

        public C7_3(int val)
        {
            Val = val;
        }
    }

    public class D7 : I7
    {
        private readonly I7 m_I7;

        public D7(I7 i7)
        {
            m_I7 = i7;
        }
    }

    public interface I8
    {
    }

    public class C8 : I8
    {
        private readonly I7[] m_I7s;

        public C8(IEnumerable<I7> i7s)
        {
            m_I7s = i7s.ToArray();
        }
    }

    public interface I9
    {
        I10 I10Inst { get; }
    }

    public class C9 : I9
    {
        public bool Val { get; }
        public I10 I10Inst { get; }

        public C9(bool val)
        {
            Val = val;
            I10Inst = new C10();
        }
    }

    public interface I10 
    {
    }

    public class C10 : I10 
    {
    }

    public interface I11 
    {
        string Test1 { get; }
    }

    public class C11 : I11 
    {
        private readonly I10 m_I10;

        public string Test1 { get; }

        public C11(I10 i10) 
        {
            m_I10 = i10;
            Test1 = "ABC";
        }
    }

    public interface I12 
    {
    }

    public class C12 : I12 
    {
        private readonly string m_Test1;

        public C12(string test1) 
        {
            m_Test1 = test1;
        }
    }

    public interface I13 
    {
    }

    public class C13 : I13 
    {
        public C13(I10 i, Dictionary<string, string> map) 
        {
        }
    }

    public class SimpleInjectorContainerBuilderTests
    {
        [Test]
        public void Test1() 
        {
            var cb1 = new SimpleInjectorContainerBuilder();

            cb1.RegisterSingleton<I1, C1>();
            cb1.RegisterInstance<I2>(new C2());
            cb1.RegisterAdapter<I9, I10>(x => x.I10Inst, LifetimeScope_e.Singleton);
            cb1.RegisterAdapter<I2, I3>(LifetimeScope_e.Singleton);
            cb1.RegisterSingleton<I4, C4>().UsingInitializer(s => s.Init());
            cb1.RegisterTransient<I5, C5>();
            cb1.RegisterSingleton<I6, C6>().UsingParameters(Parameter<string>.Named("test", "ABC"));
            cb1.RegisterDecorator<I2, D2>(LifetimeScope_e.Singleton);

            cb1.RegisterDecorator<I7, D7>(LifetimeScope_e.Singleton);
            cb1.RegisterSingleton<I8, C8>();

            cb1.RegisterSingleton<I7, C7_1>().AsCollectionItem();
            cb1.RegisterSingleton<I7, C7_2>().AsCollectionItem();
            cb1.RegisterSingleton<I7, C7_3>().UsingFactory(() => new C7_3(20)).AsCollectionItem();

            cb1.RegisterSingleton<I9, C9>().UsingFactory(() => new C9(true));

            cb1.RegisterSingleton<I11, C11>();
            cb1.RegisterSingleton<I12, C12>().UsingParameters(Parameter<string>.Indexed(0, s => s.GetService<I11>().Test1));

            var sp1 = cb1.Build();

            var s1 = sp1.GetService<I1>();
            var s2 = sp1.GetService<I2>();
            var s4 = sp1.GetService<I4>();
            var s5 = sp1.GetService<I5>();
            var s6 = sp1.GetService<I6>();
            var s8 = sp1.GetService<I8>();
            var s9 = sp1.GetService<I9>();
            var s10 = sp1.GetService<I10>();
            var s11 = sp1.GetService<I11>();
            var s12 = sp1.GetService<I12>();
        }

        [Test]
        public void Test2()
        {
            var cb1 = new SimpleInjectorContainerBuilder();
            cb1.RegisterSingleton<I10, C10>();
            cb1.RegisterSingleton<I13, C13>().UsingParameters(Parameter<Dictionary<string, string>>.Any(new Dictionary<string, string>()));

            var svc1 = cb1.Build();
            var i13 = svc1.GetService<I13>();
        }
    }
}