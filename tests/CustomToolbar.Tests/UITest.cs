//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Autofac;
using Moq;
using NUnit.Framework;
using System;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.CustomToolbar;
using Xarial.CadPlus.CustomToolbar.Services;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.CustomToolbar.UI.Forms;
using Xarial.CadPlus.CustomToolbar.UI.ViewModels;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.Commands;

namespace CustomToolbar.Tests
{
    [RequiresThread(System.Threading.ApartmentState.STA)]
    public class UITest
    {
        public class MacroEntryPointsExtractorMock : IMacroEntryPointsExtractor
        {
            public MacroStartFunction[] GetEntryPoints(string macroPath)
            {
                return new MacroStartFunction[]
                {
                    new MacroStartFunction()
                    {
                        ModuleName = "Module1",
                        SubName = "Sub1"
                    },
                    new MacroStartFunction()
                    {
                        ModuleName = "Module2",
                        SubName = "Sub2"
                    }
                };
            }
        }

        public class CustomToolbarModuleMock : CustomToolbarModule 
        {
            protected override void CreateContainer()
            {
                var builder = new ContainerBuilder();

                builder.RegisterType<MacroEntryPointsExtractorMock>()
                    .As<IMacroEntryPointsExtractor>();

                m_Container = builder.Build();
            }
        }

        [SetUp]
        public void Setup() 
        {
            var module = new CustomToolbarModuleMock();

            var extMock = new Mock<IHostExtension>();
            
            module.Init(extMock.Object);
        }

#if _RUN_UI_TESTS_
        [Test]
#endif
        public void DisplayCommandManagerView()
        {
            var toolbar = new CustomToolbarInfo();

            toolbar.Groups = new CommandGroupInfo[]
            {
                new CommandGroupInfo()
                {
                    Title = "Toolbar1",
                    Commands = new CommandMacroInfo[]
                    {
                        new CommandMacroInfo()
                        {
                            MacroPath = "D:\\1.swb",
                            Title = "Command1",
                            Description="Sample command in toolbar which will invoke some macro",
                            EntryPoint = new MacroStartFunction()
                            {
                                ModuleName = "Module1",
                                SubName = "Sub1"
                            }
                        },
                        new CommandMacroInfo() { Title = "Command2" },
                        new CommandMacroInfo() { Title = "Command3" }
                    }
                },
                new CommandGroupInfo()
                {
                    Title = "Toolbar2",
                    Commands = new CommandMacroInfo[]
                    {
                        new CommandMacroInfo() { Title = "Command4" },
                        new CommandMacroInfo() { Title = "Command5" },
                        new CommandMacroInfo() { Title = "Command6" },
                        new CommandMacroInfo() { Title = "Command7" },
                        new CommandMacroInfo() { Title = "Command8" },
                        new CommandMacroInfo() { Title = "Command9" },
                        new CommandMacroInfo() { Title = "Command10" },
                        new CommandMacroInfo() { Title = "Command11" },
                        new CommandMacroInfo() { Title = "Command12" },
                        new CommandMacroInfo() { Title = "Command13" }
                    }
                }
            };

            var confProviderMock = new Mock<IToolbarConfigurationProvider>();
            var settsProviderMock = new Mock<ISettingsProvider>();

            confProviderMock.Setup(m => m.GetToolbar(out It.Ref<bool>.IsAny, It.IsAny<string>())).
                Returns(toolbar);

            settsProviderMock.Setup(p => p.ReadSettings<ToolbarSettings>())
                .Returns(new ToolbarSettings());

            var vm = new CommandManagerVM(confProviderMock.Object, settsProviderMock.Object,
                new Mock<IMessageService>().Object, 
                new Xarial.CadPlus.Plus.Modules.IIconsProvider[0], 
                new Mock<ICadEntityDescriptor>().Object);

            var form = new CommandManagerForm();
            form.DataContext = vm;

            form.ShowDialog();
        }
    }
}
