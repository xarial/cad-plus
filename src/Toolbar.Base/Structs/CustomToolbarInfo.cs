//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.CustomToolbar.Structs
{
    public class CustomToolbarInfoVersionTransformer : BaseUserSettingsVersionsTransformer
    {
        public CustomToolbarInfoVersionTransformer()
        {
            Add(new Version(), new Version("1.0"),
                t =>
                {
                    foreach (var grp in t["Groups"].Children())
                    {
                        var prop = grp.Children<JProperty>().FirstOrDefault(p => p.Name == "Icons");
                        if (prop != null)
                        {
                            var iconPath = prop.Children().FirstOrDefault()?["IconPath"]?.ToString();
                            prop.Replace(new JProperty("IconPath", iconPath));
                        }
                    }

                    return t;
                });

            Add(new Version("1.0"), new Version("2.0"),
                t =>
                {
                    foreach (var group in t["Groups"])
                    {
                        foreach (var cmd in group["Commands"])
                        {
                            cmd["Scope"] = (1 << 0) + (2 << 0) + (3 << 0) + (4 << 0); //all
                            cmd["Triggers"] = 1 << 0; //button
                        }
                    }

                    return t;
                });

            Add(new Version("2.0"), new Version("3.0"),
                t =>
                {
                    foreach (var group in t["Groups"])
                    {
                        foreach (JObject cmd in group["Commands"])
                        {
                            cmd.Add(new JProperty("UnloadAfterRun", true));
                            cmd.Add(new JProperty("Location", (1 << 0) + (2 << 0)));
                        }
                    }

                    return t;
                });

            Add(new Version("3.0"), new Version("3.1"),
                t =>
                {
                    foreach (var group in t["Groups"])
                    {
                        foreach (JObject cmd in group["Commands"])
                        {
                            cmd.Add(new JProperty("ToggleButtonStateCodeType", 0));
                            cmd.Add(new JProperty("ToggleButtonStateCode", null));
                            cmd.Add(new JProperty("ResolveButtonStateCodeOnce", true));
                        }
                    }

                    return t;
                });

            Add(new Version("3.1"), new Version("3.2"),
                t =>
                {
                    foreach (var group in t["Groups"])
                    {
                        foreach (JObject cmd in group["Commands"])
                        {
                            cmd.Add(new JProperty("Arguments", null));
                        }
                    }

                    return t;
                });

            Add(new Version("3.2"), new Version("3.3"),
                t =>
                {
                    foreach (var group in t["Groups"])
                    {
                        foreach (JObject cmd in group["Commands"])
                        {
                            var stateCodeType = cmd.Children<JProperty>().FirstOrDefault(p => p.Name == "ToggleButtonStateCodeType");
                            
                            if (stateCodeType != null) 
                            {
                                var val = stateCodeType.Value?.ToString() != "0";
                                stateCodeType.Replace(new JProperty("EnableToggleButtonStateExpression", val));
                            }

                            var stateCodeExp = cmd.Children<JProperty>().FirstOrDefault(p => p.Name == "ToggleButtonStateCode");

                            if (stateCodeExp != null)
                            {
                                stateCodeExp.Replace(new JProperty("ToggleButtonStateExpression", stateCodeExp.Value));
                            }

                            var stateCodeResOnce = cmd.Children<JProperty>().FirstOrDefault(p => p.Name == "ResolveButtonStateCodeOnce");

                            if (stateCodeResOnce != null)
                            {
                                stateCodeResOnce.Replace(new JProperty("CacheToggleState", stateCodeResOnce.Value));
                            }
                        }
                    }

                    return t;
                });
        }
    }

    [UserSettingVersion("3.3", typeof(CustomToolbarInfoVersionTransformer))]
    public class CustomToolbarInfo
    {
        public CommandGroupInfo[] Groups { get; set; }

        public CustomToolbarInfo()
        {
        }

        internal CustomToolbarInfo Clone()
            => new CustomToolbarInfo()
            {
                Groups = Groups?.Select(g => g.Clone()).ToArray()
            };
    }
}