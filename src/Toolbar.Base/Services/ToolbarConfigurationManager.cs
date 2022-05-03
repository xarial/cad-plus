//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.CustomToolbar.Services;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Base;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.Toolbar.Services
{
    public interface IToolbarConfigurationManager 
    {
        string FilePath { get; set; }
        CustomToolbarInfo Toolbar { get; set; }

        void Load();

        bool SettingsChanged { get; }
        bool ToolbarChanged { get; }
        void SaveSettings();
        void SaveToolbar();
    }

    public class ToolbarConfigurationManager : IToolbarConfigurationManager
    {
        public CustomToolbarInfo Toolbar { get; set; }

        public string FilePath
        {
            get => m_Settings.SpecificationFile;
            set => m_Settings.SpecificationFile = value;
        }

        public bool SettingsChanged => !DeepCompare(m_Settings, m_SettingsOrig);
        public bool ToolbarChanged => !DeepCompare(Toolbar, m_ToolbarOrig);

        private ToolbarSettings m_Settings;

        private readonly IXLogger m_Logger;
        private readonly IToolbarConfigurationProvider m_ConfigProvider;
        private readonly ISettingsProvider m_SettsProvider;
        private readonly UserSettingsService m_UserSettsSrv;

        private ToolbarSettings m_SettingsOrig;
        private CustomToolbarInfo m_ToolbarOrig;

        public ToolbarConfigurationManager(IToolbarConfigurationProvider configProvider, ISettingsProvider settsProvider, 
            UserSettingsService userSettsSrv, IXLogger logger) 
        {
            m_ConfigProvider = configProvider;
            m_SettsProvider = settsProvider;
            m_UserSettsSrv = userSettsSrv;
            m_Logger = logger;
        }

        public void Load()
        {
            m_Settings = m_SettsProvider.ReadSettings<ToolbarSettings>();
            m_SettingsOrig = m_Settings.Clone();

            Toolbar = m_ConfigProvider.GetToolbar(FilePath);
            m_ToolbarOrig = Toolbar.Clone();
        }

        public void SaveSettings()
            => m_SettsProvider.WriteSettings(m_Settings);

        public void SaveToolbar()
        {
            UpdateGroupIds(Toolbar.Groups, m_ToolbarOrig.Groups);
            m_UserSettsSrv.StoreSettings(Toolbar, FilePath);
        }

        private void UpdateGroupIds(CommandGroupInfo[] curCmdGroups, CommandGroupInfo[] oldCmdGroups)
        {
            var usedIds = curCmdGroups.Select(g => g.Id).ToList();

            int GetAvailableGroupId()
            {
                int id = 1;

                while (usedIds.Contains(id))
                {
                    id++;
                }

                usedIds.Add(id);

                return id;
            }

            if (curCmdGroups != null && oldCmdGroups != null)
            {
                foreach (var curCmdGrp in curCmdGroups)
                {
                    var corrCmdGrp = oldCmdGroups.FirstOrDefault(g => g.Id == curCmdGrp.Id);

                    if (corrCmdGrp != null)
                    {
                        if (curCmdGrp.Commands != null && corrCmdGrp.Commands != null)
                        {
                            if (!curCmdGrp.Commands.Select(c => c.Id).OrderBy(x => x)
                                .SequenceEqual(corrCmdGrp.Commands.Select(c => c.Id).OrderBy(x => x)))
                            {
                                var newId = GetAvailableGroupId();

                                m_Logger.Log($"Changing id of the group from {curCmdGrp.Id} to {newId}", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                                curCmdGrp.Id = newId;
                            }
                        }
                    }
                }
            }
        }

        private bool DeepCompare(object obj1, object obj2)
            => JToken.DeepEquals(JToken.FromObject(obj1), JToken.FromObject(obj2));
    }
}
