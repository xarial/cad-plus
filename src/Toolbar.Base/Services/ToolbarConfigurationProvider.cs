//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface IToolbarConfigurationProvider
    {
        CustomToolbarInfo GetToolbar(string toolbarSpecFilePath);
        bool IsReadOnly(string toolbarSpecFilePath);
    }

    public class ToolbarConfigurationProvider : IToolbarConfigurationProvider
    {
        private readonly UserSettingsService m_UserSettsSrv;

        public ToolbarConfigurationProvider(UserSettingsService userSettsSrv)
        {
            m_UserSettsSrv = userSettsSrv;
        }

        public CustomToolbarInfo GetToolbar(string toolbarSpecFilePath)
        {
            if (File.Exists(toolbarSpecFilePath))
            {
                return m_UserSettsSrv.ReadSettings<CustomToolbarInfo>(toolbarSpecFilePath);
            }
            else
            {
                return new CustomToolbarInfo();
            }
        }

        public bool IsReadOnly(string toolbarSpecFilePath)
        {
            if (File.Exists(toolbarSpecFilePath))
            {
                return !IsEditable(toolbarSpecFilePath);
            }
            else
            {
                return false;
            }
        }

        private bool IsEditable(string filePath)
        {
            if (!new FileInfo(filePath).IsReadOnly)
            {
                var permissionSet = new PermissionSet(PermissionState.None);
                var writePermission = new FileIOPermission(
                    FileIOPermissionAccess.Write, filePath);
                permissionSet.AddPermission(writePermission);

                return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
            }
            else
            {
                return false;
            }
        }
    }
}