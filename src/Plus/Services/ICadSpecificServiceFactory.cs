//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XToolkit.Reflection;

namespace Xarial.CadPlus.Plus.Services
{
    public interface ICadSpecificServiceFactory<TService>
    {
        TService GetService(string cadId);
    }

    public class CadSpecificServiceFactory<TService> : ICadSpecificServiceFactory<TService>
    {
        private readonly Dictionary<string, TService> m_Services;

        public CadSpecificServiceFactory(IEnumerable<TService> services) 
        {
            m_Services = new Dictionary<string, TService>(StringComparer.CurrentCultureIgnoreCase);

            foreach (var svc in services) 
            {
                if (!svc.GetType().TryGetAttribute<CadSpecificServiceAttribute>(out var att)) 
                {
                    throw new Exception($"CAD specific service of type '{svc.GetType().FullName}' must be decorated with '{nameof(CadSpecificServiceAttribute)}' attribute");
                }

                var cadId = att.CadId;

                if (!string.IsNullOrEmpty(cadId))
                {
                    if (!m_Services.ContainsKey(cadId))
                    {
                        m_Services.Add(cadId, svc);
                    }
                    else 
                    {
                        throw new Exception($"Duplicate CAD specific service: '{svc.GetType().FullName}'");
                    }
                }
                else 
                {
                    throw new Exception("CAD Id is empty");
                }
            }
        }

        public TService GetService(string cadId)
        {
            if (m_Services.TryGetValue(cadId, out var svc))
            {
                return svc;
            }
            else 
            {
                throw new Exception("CAD specific service is not registered");
            }
        }
    }
}
