using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Plus.Shared.Helpers
{
    public class RotRegister : IDisposable
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int CreateItemMoniker([MarshalAs(UnmanagedType.LPWStr)] string lpszDelim,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszItem, out IMoniker ppmk);

        public string Name { get; }
        public int Id { get; }

        private IXLogger m_Logger;

        public RotRegister(object obj, string name, IXLogger logger)
        {
            Name = name;
            m_Logger = logger;

            m_Logger.Log($"Registered COM Object as '{name}'", LoggerMessageSeverity_e.Debug);

            Id = RegisterComObject(obj, name, true, false, m_Logger);
        }

        public void Dispose()
        {
            m_Logger.Log($"Unregistering object from ROT: {Id}", LoggerMessageSeverity_e.Debug);

            UnregisterComObject(Id);
        }

        /// <summary>
        /// Registers an object in the ROT table
        /// </summary>
        /// <param name="obj">COM Object</param>
        /// <param name="monikerName">Name of the moniker</param>
        /// <param name="keepAlive">Keep object alive</param>
        /// <param name="allowAnyClient">Allow any client to connect</param>
        /// <param name="logger">Logger</param>
        /// <returns></returns>
        private int RegisterComObject(object obj, string monikerName, bool keepAlive = true, bool allowAnyClient = false, IXLogger logger = null)
        {
            IMoniker moniker = null;

            IBindCtx context;
            CreateBindCtx(0, out context);

            IRunningObjectTable rot;
            context.GetRunningObjectTable(out rot);

            try
            {
                const int ROTFLAGS_REGISTRATIONKEEPSALIVE = 1;
                const int ROTFLAGS_ALLOWANYCLIENT = 2;

                context.GetRunningObjectTable(out rot);

                const int S_OK = 0;

                if (CreateItemMoniker("", monikerName, out moniker) != S_OK)
                {
                    throw new Exception("Failed to create moniker");
                }

                var opts = 0;

                if (keepAlive)
                {
                    opts += ROTFLAGS_REGISTRATIONKEEPSALIVE;
                }

                if (allowAnyClient)
                {
                    opts += ROTFLAGS_ALLOWANYCLIENT;
                }

                logger?.Log($"Registering object in ROT with {opts} option", LoggerMessageSeverity_e.Debug);

                var id = rot.Register(opts, obj, moniker);

                if (id == 0)
                {
                    throw new Exception("Failed to register object in ROT");
                }

                logger?.Log($"Object id in ROT: {id}", LoggerMessageSeverity_e.Debug);

                return id;
            }
            catch (Exception ex)
            {
                logger?.Log(ex);
                throw;
            }
            finally
            {
                if (moniker != null)
                {
                    while (Marshal.ReleaseComObject(moniker) > 0) ;
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

        /// <summary>
        /// Unregisters COM object from the Running Objects Table
        /// </summary>
        /// <param name="id">Id of the object</param>
        /// <param name="logger">Logger</param>
        private void UnregisterComObject(int id, IXLogger logger = null)
        {
            IBindCtx context;
            CreateBindCtx(0, out context);

            IRunningObjectTable rot;
            context.GetRunningObjectTable(out rot);

            try
            {
                rot.Revoke(id);
            }
            catch (Exception ex)
            {
                logger?.Log(ex);
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
