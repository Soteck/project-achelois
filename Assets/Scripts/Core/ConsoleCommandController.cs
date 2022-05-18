using System.Collections.Generic;
using System.Linq;
using Core.Commands;

namespace Core {
    public class ConsoleCommandController : Singleton<ConsoleCommandController> {

        private readonly List<ICommand> _commands = new List<ICommand>() {
            new QuitCommand(),
            new ConnectCommand(),
            new ServerCommand(),
            new ConfigCommand()
        };


        private void DoExecuteCommand(string commandTxt) {
            string[] splitCommands = commandTxt.Split(' ');
            foreach (ICommand command in _commands) {
                if (command.Command().Equals(splitCommands[0])) {
                    if (splitCommands.Length < 2) {
                        command.Execute(null);
                    } else {
                        command.Execute( splitCommands.Skip(1).ToArray());
                    }
                    return;
                }
            }
            Logger.Warning("Command not recognized: " + commandTxt);
        }
        
        public static void ExecuteCommand(string command) {
            ConsoleCommandController.Instance.DoExecuteCommand(command);
        }
    }
}