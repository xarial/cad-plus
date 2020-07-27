using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Xarial.CadPlus.XToolbar.Structs;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.XToolbar.Services
{
    public interface IToolbarConfigurationProvider
    {
        CustomToolbarInfo GetToolbar(out bool isReadOnly, string toolbarSpecFilePath);

        void SaveToolbar(CustomToolbarInfo toolbar, string toolbarSpecFilePath);
    }

    public class ToolbarConfigurationProvider : IToolbarConfigurationProvider
    {
        private readonly UserSettingsService m_UserSettsSrv;

        public ToolbarConfigurationProvider(UserSettingsService userSettsSrv)
        {
            m_UserSettsSrv = userSettsSrv;
        }

        public CustomToolbarInfo GetToolbar(out bool isReadOnly, string toolbarSpecFilePath)
        {
            if (File.Exists(toolbarSpecFilePath))
            {
                isReadOnly = !IsEditable(toolbarSpecFilePath);
                return m_UserSettsSrv.ReadSettings<CustomToolbarInfo>(toolbarSpecFilePath);
            }
            else
            {
                isReadOnly = false;
                return new CustomToolbarInfo();
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

        public void SaveToolbar(CustomToolbarInfo toolbar, string toolbarSpecFilePath)
        {
            m_UserSettsSrv.StoreSettings(toolbar, toolbarSpecFilePath);
        }
    }
}