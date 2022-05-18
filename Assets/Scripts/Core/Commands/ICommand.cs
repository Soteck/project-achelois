namespace Core.Commands {
    public interface ICommand {
        public void Execute(string[] args);
        public string Command();
    }
}