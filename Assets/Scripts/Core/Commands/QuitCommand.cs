using UnityEngine;

namespace Core.Commands {
    public class QuitCommand : ICommand {
        
        public void Execute(string[] args) {
            Application.Quit();
        }

        public string Command() {
            return "quit";
        }
    }
}