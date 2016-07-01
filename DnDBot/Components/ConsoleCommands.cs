namespace DnDBot
{
    public partial class Program
    {
        void ConsoleCommand()
        {
            string Command = CommandLine.Split(' ')[0];

            switch (Command.ToLower())
            {
                case "quit":
                    Quit();
                    break;
                case "say":
                    SayInChannel(CommandLine.Substring(Command.Length + 1));
                    break;
                case "do":
                    SayInChannel("\x0001ACTION " + CommandLine.Substring(Command.Length + 1));
                    break;
            }
        }

        void Quit()
        {
            SayInChannel(BeforeLeave);
            DCTimer = 150;
        }
    }
}
