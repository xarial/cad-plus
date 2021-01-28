//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.CadPlus.Common.Services
{
    public interface IPropertyPageCreator
    {
        IXPropertyPage<TData> CreatePage<TData>();
    }

    public class PropertyPageCreator : IPropertyPageCreator
    {
        private readonly IXExtension m_Ext;

        public PropertyPageCreator(IXExtension ext) 
        {
            m_Ext = ext;
        }

        public IXPropertyPage<TData> CreatePage<TData>() => m_Ext.CreatePage<TData>();
    }
}
