using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Atributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XCad.Reflection;

namespace Xarial.CadPlus.Plus.Services
{
    public class ModulesSorter
    {
        public IModule[] Sort(IModule[] modules)
        {
            var modulePositions = new Dictionary<Guid, Tuple<IModule, int>>();
            var moduleRelativePositions = new Dictionary<IModule, Tuple<Guid, ModuleRelativeOrder_e>>();

            foreach (var module in modules)
            {
                var pos = ExtractPosition(module, out Tuple<Guid, ModuleRelativeOrder_e> relOrder);

                if (pos.HasValue)
                {
                    modulePositions.Add(module.Id, new Tuple<IModule, int>(module, pos.Value));
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

                    if (modulePositions.TryGetValue(modRelPos.Value.Item1, out Tuple<IModule, int> modPos))
                    {
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
                    else if (moduleRelativePositions.Keys.FirstOrDefault(m => m.Id == modRelPos.Value.Item1) == null)
                    {
                        pos = 0;
                    }

                    if (pos.HasValue)
                    {
                        moduleRelativePositions.Remove(modRelPos.Key);
                        hasChanges = true;
                        modulePositions.Add(modRelPos.Key.Id,
                            new Tuple<IModule, int>(modRelPos.Key, pos.Value));
                    }
                }
            }

            return modulePositions.OrderBy(x => x.Value.Item2).Select(x => x.Value.Item1).ToArray();
        }

        private int? ExtractPosition(IModule module, out Tuple<Guid, ModuleRelativeOrder_e> rel)
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
                    if (string.IsNullOrEmpty(att.RelativeToModuleId) || !att.RelativeOrder.HasValue)
                    {
                        throw new Exception($"Order is not set for module '{module.Id}'");
                    }

                    rel = new Tuple<Guid, ModuleRelativeOrder_e>(Guid.Parse(att.RelativeToModuleId), att.RelativeOrder.Value);
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
