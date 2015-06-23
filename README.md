# Roanbot
This is the base code for Roanbot, plus the module base. In order for Roanbot to work, you need:

1. Roanbot.exe
2. ModuleBase.dll in the same directory as Roanbot.exe
3. Skype4COM.dll in the same directory as Roanbot.exe
4. Roanbot will now run, but will not be useful unless you have BasicCommands.dll in the Modules folder.

By placing a .dll in the Updates folder, Roanbot will automaticall move it to the Modules folder and replace any old .dll of the same name. This can be achieved during runtime by having the owner give the command "!reload". This will cause Roanbot to exit and start up again after 1 second.
