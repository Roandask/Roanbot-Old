using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModuleBase
{
    public class ModuleBase
    {
        public ModuleBase()
        {
        }

        public abstract void addReferences();

        public abstract void processCommand();
    }
}
