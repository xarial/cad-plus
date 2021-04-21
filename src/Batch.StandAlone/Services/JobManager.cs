//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IJobManager : IDisposable 
    {
        void AddProcess(Process process);
    }

    public class JobManager : IJobManager
    {
        #region WinAPI

        [StructLayout(LayoutKind.Sequential)]
        public struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public uint nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        public enum JobObjectInfoType
        {
            AssociateCompletionPortInformation = 7,
            BasicLimitInformation = 2,
            BasicUIRestrictions = 4,
            EndOfJobTimeInformation = 6,
            ExtendedLimitInformation = 9,
            SecurityLimitInformation = 5,
            GroupInformation = 11
        }

        #endregion

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr a, string lpName);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, UInt32 cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private IntPtr m_JobHandle;

        private bool m_IsInit;

        private readonly IXLogger m_Logger;

        public JobManager(IXLogger logger) 
        {
            m_Logger = logger;
            m_IsInit = false;
        }

        public void Init()
        {
            try
            {
                if (!m_IsInit)
                {
                    m_JobHandle = CreateJobObject(IntPtr.Zero, null);

                    if (m_JobHandle == IntPtr.Zero)
                    {
                        throw new Exception("Failed to create job handle");
                    }

                    var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION()
                    {
                        BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION()
                        {
                            LimitFlags = 0x2000
                        }
                    };

                    var length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                    var extendedInfoPtr = Marshal.AllocHGlobal(length);

                    Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                    if (!SetInformationJobObject(m_JobHandle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
                    {
                        throw new Exception(string.Format("Unable to set information.  Error: {0}", Marshal.GetLastWin32Error()));
                    }

                    m_Logger.Log($"Job initiated: {m_JobHandle}", LoggerMessageSeverity_e.Debug);
                    m_IsInit = true;
                }
                else
                {
                    throw new Exception("Job is already initialized");
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                throw;
            }
        }

        public void AddProcess(Process process)
        {
            if (m_IsInit)
            {
                if (!AssignProcessToJobObject(m_JobHandle, process.Handle))
                {
                    throw new Exception($"Failed to assign process to job: {process.Id}");
                }

                m_Logger.Log($"Added process to job: {process.Id}", LoggerMessageSeverity_e.Debug);
            }
            else 
            {
                throw new Exception("Job object is not initialized");
            }
        }

        public void Dispose()
        {
            m_Logger.Log("Disposing job manager", LoggerMessageSeverity_e.Debug);

            CloseHandle(m_JobHandle);
            m_JobHandle = IntPtr.Zero;

            GC.SuppressFinalize(this);
        }
    }
}
