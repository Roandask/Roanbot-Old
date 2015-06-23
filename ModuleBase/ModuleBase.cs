using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKYPE4COMLib;


namespace ModuleBase
{
    /// <summary>
    /// Registers commands to Roanbot and processes the commands it gets sent. Should only be sent commands it registered.
    /// Extends ModuleBase.
    /// </summary>
    public abstract class ModuleBase
    {
        /// <summary>
        /// Invokes the main class' addCommand method with commands this class will handle.
        /// </summary>
        /// <param name="obj">The main class that handles Skype messages.</param>
        public abstract void addReferences(object obj);

        /// <summary>
        /// Processes a Skype message command. This function should only be run if the command is registered by the class, otherwise
        /// it may not function correctly.
        /// </summary>
        /// <param name="skype">The current Skype instance.</param>
        /// <param name="msg">The message to be processed by the function.</param>
        public abstract void processCommands(Skype skype, ChatMessage msg);
    }
}
