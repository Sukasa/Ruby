using DnDBot.Properties;
using IrcDotNet;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace DnDBot
{
    public partial class Program
    {
        Random RNG = new Random(Guid.NewGuid().GetHashCode());
        StandardIrcClient Client;

        string CommandLine = "";
        int DCTimer = -1;

        static void Main(string[] args)
        {
            (new Program()).Run();
        }

        void PrintLine(string Line, ConsoleColor Color = ConsoleColor.Gray)
        {
            if (string.IsNullOrWhiteSpace(Line))
                return;

            int NumLines = (int)Math.Ceiling(Line.Length / (double)Console.WindowWidth);

            Console.MoveBufferArea(0, NumLines, Console.WindowWidth, Console.WindowHeight - (NumLines + 1), 0, 0);

            for (int x = 1; x <= NumLines; x++)
            {
                Console.SetCursorPosition(0, Console.WindowHeight - (x + 1));
                Console.Write(new string(' ', Console.WindowWidth));
            }

            Console.SetCursorPosition(0, Console.WindowHeight - (NumLines + 1));
            Console.ForegroundColor = Color;
            Console.Write(Line);
            Console.SetCursorPosition(Math.Min(CommandLine.Length, Console.WindowWidth - 1), Console.WindowHeight - 1);
        }

        void DrawCommandLine()
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            int Skip = CommandLine.Length - (Console.WindowWidth - 1);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkGray;

            Console.Write(CommandLine.Substring(Math.Max(0, Skip)));

            if (Skip < 0)
                Console.Write(new string(' ', (-Skip) + 1));

            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(0, 0);
            Console.SetCursorPosition(Math.Min(CommandLine.Length, Console.WindowWidth - 1), Console.WindowHeight - 1);
        }

        void Run()
        {
            Client = new StandardIrcClient();
            DrawCommandLine();

            var Info = new IrcUserRegistrationInfo();
            Info.NickName = Settings.Default.Name;
            Info.RealName = Settings.Default.Realname;
            Info.UserName = Settings.Default.Realname + "Bot";

            using (var connectedEvent = new ManualResetEventSlim(false))
            {
                Client.Connected += (sender2, e2) => connectedEvent.Set();

                Client.Connect(new Uri(Settings.Default.Server), Info);

                if (!connectedEvent.Wait(10000))
                {
                    Client.Dispose();
                    PrintLine(string.Format("Connection to {0} timed out.", Settings.Default.Server), ConsoleColor.Red);
                    return;
                }

                PrintLine(string.Format("Connected to {0}", Settings.Default.Server));

                // *** POST-INIT
                Client.MotdReceived += delegate (Object Sender, EventArgs E)
                {
                    PrintLine("Joining Channels...");
                    Client.Channels.Join(Settings.Default.Channel);
                };

                // *** DEBUG OUTPUT
                Client.RawMessageReceived += delegate (Object Sender, IrcRawMessageEventArgs Event)
                {
                    PrintLine(Event.RawContent);
                };

                // *** PING
                Client.PingReceived += delegate (Object Sender, IrcPingOrPongReceivedEventArgs Event)
                {
                    Client.Ping(Event.Server);
                };

                // *** CHANNEL JOINING
                Client.LocalUser.JoinedChannel += delegate (Object Sender, IrcChannelEventArgs Event)
                {
                    Event.Channel.MessageReceived += Channel_MessageReceived;
                    SayInChannel(OnJoinActions);
                };

                // *** REJOIN AFTER KICK
                Client.LocalUser.LeftChannel += delegate (Object Sender, IrcChannelEventArgs Event)
                {
                    Client.Channels.Join(Event.Channel.Name);
                };

                Int32 Counter = 0;
                while (Client.IsConnected)
                {
                    Thread.Sleep(5);
                    if (DCTimer > 0)
                    {
                        DCTimer--;
                        if (DCTimer == 0)
                            Client.Disconnect();
                    }
                    if (++Counter == 12000)
                    {
                        PrintLine("Manual Ping");
                        Client.Ping();
                        Counter = 0;
                    }
                    while (Console.KeyAvailable)
                    {
                        Console.SetCursorPosition(0, 0);
                        ConsoleKeyInfo Key = Console.ReadKey(true);
                        switch (Key.Key)
                        {
                            case ConsoleKey.Enter:
                                ConsoleCommand();
                                CommandLine = "";
                                break;
                            case ConsoleKey.Backspace:
                                CommandLine = CommandLine.Substring(0, CommandLine.Length - 1);
                                break;
                            default:
                                CommandLine = CommandLine + Key.KeyChar;
                                break;
                        }
                        DrawCommandLine();
                    }
                }

            }
            DrawCommandLine();
        }

        static Regex DiceRegex = new Regex(@"!(?:(?:roll )?(\d+)d([^{]\S*|{[^}]+}))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex RecallRegex = new Regex(@"!recall\s+(\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex NoteRegex = new Regex(@"!note\s+(\S+)\s+(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex LogRegex = new Regex(@"!log\s+((?:([^#]|\w+)\s*)+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex HistoryRegex = new Regex(@"!history((?:\s+#\w+)*)(?:\s+page=(\d+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        void Channel_MessageReceived(object Sender, IrcMessageEventArgs Event)
        {
            IrcChannel Channel = (IrcChannel)Sender;
            string Text = Event.Text;

            if (Text.ToLower().Contains("!hashtags"))
            {
                SendHashTags(Event.Source.Name);
            }
            else if (Text.ToLower().Contains("!help"))
            {
                SayInChannel("Do I look like I come with a manual?");
            }
            else if (DiceRegex.IsMatch(Text))
            {
                Match T = DiceRegex.Match(Text);
                DiceRoll(T.Groups[1].Value, T.Groups[2].Value);
            }
            else if (RecallRegex.IsMatch(Text))
            {
                Match T = RecallRegex.Match(Text);
                Recall(T.Groups[1].Value);
            }
            else if (NoteRegex.IsMatch(Text))
            {
                Match T = NoteRegex.Match(Text);
                Note(T.Groups[1].Value, T.Groups[2].Value);
            }
            else if (LogRegex.IsMatch(Text))
            {
                Match T = LogRegex.Match(Text);
                Log(T.Groups[1].Value, Text);
            }
            else if (HistoryRegex.IsMatch(Text))
            {
                Match T = HistoryRegex.Match(Text);
                SearchByHashTags(T.Groups[1].Value, T.Groups[2].Value);
            }
        }
    }
}