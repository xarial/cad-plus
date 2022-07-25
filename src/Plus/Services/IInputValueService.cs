using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IInputValueService
    {
        bool PromptInput(string title, string prompt, out string value);
    }
}