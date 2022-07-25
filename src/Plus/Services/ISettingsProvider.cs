//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

namespace Xarial.CadPlus.Plus.Services
{
    public interface ISettingsProvider
    {
        T ReadSettings<T>()
            where T : new();

        void WriteSettings<T>(T setts)
            where T : new();
    }
}
