//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XCad.Reflection;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class ModulesSorter
    {
        public IModule[] Sort(IModule[] modules)
        {
            var modulePositions = new Dictionary<Type, Tuple<IModule, int>>();
            var moduleRelativePositions = new Dictionary<IModule, Tuple<Type, ModuleRelativeOrder_e>>();

            foreach (var module in modules)
            {
                var pos = ExtractPosition(module, out Tuple<Type, ModuleRelativeOrder_e> relOrder);

                if (pos.HasValue)
                {
                    modulePositions.Add(module.GetType(), new Tuple<IModule, int>(module, pos.Value));
                }
                else
                {
                    moduleRelativePositions.Add(module, relOrder);
                }
            }

            bool hasChanges = true;

            while (moduleRelativePositions.Any())
            {
                if (!hasChanges)
                {
                    throw new ModuleOrderCircularDependencyException(moduleRelativePositions.Keys);
                }

                hasChanges = false;

                foreach (var modRelPos in moduleRelativePositions.ToArray())
                {
                    int? pos = null;

                    var key = modulePositions.Keys.FirstOrDefault(t => modRelPos.Value.Item1.IsAssignableFrom(t));

                    if (key != null)
                    {
                        var modPos = modulePositions[key];

                        switch (modRelPos.Value.Item2)
                        {
                            case ModuleRelativeOrder_e.Before:
                                pos = modPos.Item2 - 1;
                                break;

                            case ModuleRelativeOrder_e.After:
                                pos = modPos.Item2 + 1;
                                break;

                            default:
                                throw new NotSupportedException();
                        }
                    }
                    else if (moduleRelativePositions.Keys.FirstOrDefault(m => modRelPos.Value.Item1.IsAssignableFrom(m.GetType())) == null)
                    {
                        pos = 0;
                    }

                    if (pos.HasValue)
                    {
                        moduleRelativePositions.Remove(modRelPos.Key);
                        hasChanges = true;
                        modulePositions.Add(modRelPos.Key.GetType(),
                            new Tuple<IModule, int>(modRelPos.Key, pos.Value));
                    }
                }
            }

            return modulePositions.OrderBy(x => x.Value.Item2).Select(x => x.Value.Item1).ToArray();
        }

        private int? ExtractPosition(IModule module, out Tuple<Type, ModuleRelativeOrder_e> rel)
        {
            rel = null;
            ModuleOrderAttribute att = null;
            
            if (module.GetType().TryGetAttribute<ModuleOrderAttribute>(a => att = a))
            {
                if (att.Order.HasValue)
                {
                    return att.Order;
                }
                else
                {
                    if (att.RelativeToModuleType == null || !att.RelativeOrder.HasValue)
                    {
                        throw new Exception($"Order is not set for module '{module.GetType().FullName}'");
                    }

                    rel = new Tuple<Type, ModuleRelativeOrder_e>(att.RelativeToModuleType, att.RelativeOrder.Value);
                    return null;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
