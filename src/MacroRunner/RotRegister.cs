//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Xarial.CadPlus.MacroRunner
{
    internal class RotRegister : IDisposable
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int CreateItemMoniker([MarshalAs(UnmanagedType.LPWStr)] string
            lpszDelim, [MarshalAs(UnmanagedType.LPWStr)] string lpszItem,
            out IMoniker ppmk);

        private readonly List<int> m_RegisteredIds;

        internal RotRegister() 
        {
            m_RegisteredIds = new List<int>();
        }

        internal void RegisterObject(object obj, string name)
        {
            IBindCtx context = null;
            IRunningObjectTable rot = null;
            IMoniker moniker = null;

            CreateBindCtx(0, out context);
            context.GetRunningObjectTable(out rot);

            try
            {
                const int ROTFLAGS_REGISTRATIONKEEPSALIVE = 1;

                context.GetRunningObjectTable(out rot);

                const int S_OK = 0;

                if (CreateItemMoniker("", name, out moniker) != S_OK) 
                {
                    throw new Exception("Failed to create moniker");
                }

                var id = rot.Register(ROTFLAGS_REGISTRATIONKEEPSALIVE, obj, moniker);

                if (id == 0) 
                {
                    throw new Exception("Failed to register object in ROT");
                }

                m_RegisteredIds.Add(id);
            }
            finally
            {
                if (moniker != null)
                {
                    while (Marshal.ReleaseComObject(moniker) > 0);
                }
                if (rot != null)
                {
                    while (Marshal.ReleaseComObject(rot) > 0) ;
                }
                if (context != null)
                {
                    while (Marshal.ReleaseComObject(context) > 0) ;
                }
            }
        }

        public void Dispose()
        {
            IBindCtx context = null;
            IRunningObjectTable rot = null;

            CreateBindCtx(0, out context);
            context.GetRunningObjectTable(out rot);

            try
            {
                foreach (var id in m_RegisteredIds)
                {
                    try
                    {
                        rot.Revoke(id);
                    }
                    catch
                    {
                    }
                }
            }
            finally
            {
                if (rot != null)
                {
                    while (Marshal.ReleaseComObject(rot) > 0);
                }

                if (context != null)
                {
                    while (Marshal.ReleaseComObject(context) > 0);
                }
            }
        }
    }
}
