//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.CadPlus.CustomToolbar.Base
{
    internal class CommandItemInfoSpec : CommandSpec
    {
        internal CommandMacroInfo Info { get; }

        internal CommandItemInfoSpec(CommandMacroInfo info, IReadOnlyList<IIconsProvider> iconsProviders, IFilePathResolver pathResolver, string workDir) : base(info.Id)
        {
            Info = info;

            Title = info.Title;
            Tooltip = info.Description;
            Icon = info.GetCommandIcon(iconsProviders, pathResolver, workDir);
            HasToolbar = info.Location.HasFlag(Location_e.Toolbar);
            HasMenu = info.Location.HasFlag(Location_e.Menu);
            HasRibbon = info.Location.HasFlag(Location_e.TabBox);
            SupportedWorkspace = GetWorkspace(info.Scope);

            if (HasRibbon)
            {
                RibbonTextStyle = RibbonTabTextDisplay_e.TextBelow;
            }
        }

        private WorkspaceTypes_e GetWorkspace(MacroScope_e scope)
        {
            WorkspaceTypes_e workspace = 0;

            if (scope.HasFlag(MacroScope_e.Application))
            {
                workspace |= WorkspaceTypes_e.NoDocuments;
            }

            if (scope.HasFlag(MacroScope_e.Part))
            {
                workspace |= WorkspaceTypes_e.Part;
            }

            if (scope.HasFlag(MacroScope_e.Assembly))
            {
                workspace |= WorkspaceTypes_e.Assembly;
            }

            if (scope.HasFlag(MacroScope_e.Drawing))
            {
                workspace |= WorkspaceTypes_e.Drawing;
            }

            return workspace;
        }
    }
}