namespace DnDBot
{
    public partial class Program
    {
        static string[] CantDoThat = new string[] {
            "And I'm going to manage that, how?", "I can't do that, genius.", "Uh, no.  Make sense first.", "You're really good at this.  Try again.",
            "Um...?", "Sorry, I'm not sure I understand that one.", "Sorry, I'm not HAL, but I'm afraid I can't do that", "You're gonna have to run that one past me again.", "Try making sense first.",
            "I don't think I can?", "No.", "Look, I know I don't come with a manual but c'mon.", "\x0001ACTION headdesks", "\x0001ACTION looks at you flatly", "\x0001ACTION sighs", "\x0001ACTION shakes her head and sighs"
        };

        static string[] Searching = new string[]
        {
            "\x0001ACTION rustles through her notebook", "\x0001ACTION fumbles through her notes, spilling a page on the floor", "\x0001ACTION flips through a few pages", "Gimme a second here..",
            "Let's see what I can find", "Hmmm", "Needle in a bloody haystack", "mm."
        };

        static string[] InfoSaved = new string[] {
            "Got it.", "Written down!", "\x0001ACTION scribbles in her notebook", "\x0001ACTION nods", "Gotcha.", "K", "mm.", "Mmhmm.", "Yep.", "Alright.", "No problem.", "Done.", "Saved.", "On paper.", "Not gonna forget it!"
        };

        static string[] SearchFail = new string[]
        {
            "I got nothing under that", "Can't find that one", "I don't have anything written down for that", "Nope, no go", "No go", "Sorry, nothing.", "\x0001ACTION frowns and shakes her head.  \"Nada.\"",
            "Can't find anything", "Not finding what you're looking for.", "Sorry, no go", "Nilch"
        };

        static string[] OutOfRange = new string[]
        {
            "That's a little excessive, isn't it?", "That's a bit much.", "No, you do that.", "Really?", "\x00001ACTION groans.", "Give me a break.", "I don't have time for that.", "\x0001ACTION shakes her head and sighs"
        };
        
        static string[] DiceRollText = new string[]
        {
            "Rolling...", "You got", "And the die says", "", "Looks like you rolled", "It's", "Boom. ", "And... ", "Surprise, it's", "You get", "\x0001ACTION rolls", "\x0001ACTION rolls and gets", "\x0001ACTION calls out"
        };

        static string[] OnJoinActions = new string[]
        {
            "Hi.", "\x0001ACTION puts on her robe and wizard hat", "You guys ready?", "Let's get this party started.", "Hang on, lemme get my notebook out.", "Did anyone bring chips?", "I'd have brought the dip but I forgot it, sorry.",
            "Let's try not to burn something down today.", "Hey hey~", "'Sup, dudes and dudettes?"
        };

        static string[] BeforeLeave = new string[]
        {
            "Bye!", "See you guys around.", "Take care, nerds.", "Peace!", "I'm gonna need a drink after this.", "Cya.", "Don't get eaten by a grue!"
        };

        static string[] ListHashtags = new string[]
        {
            "These are the hashtags we've used before.  Try to keep yourself in check.", "\x0001ACTION shows you her hashtag list",
            "Here're the hashtags we've used.  Do me a favour, don't make my job harder.", "Hashtags, get yer Hashtags.  Social Media special, extra mediocre!",
            "Here, take a look", "\x0001ACTION passes you her list of hashtags you've used before", "Here, have a look."
        };

        //-------------------------

        void SayInChannel(string[] Messages)
        {
            SayInChannel(Messages[RNG.Next(Messages.Length)]);
        }

        void SayInChannel(string Message)
        {
            foreach (var Channel in Client.Channels)
                Client.LocalUser.SendMessage(Channel.Name, Message);
        }

        void MessagePlayer(string PlayerName, string Message)
        {
            Client.LocalUser.SendMessage(PlayerName, Message);
        }

        void MessagePlayer(string PlayerName, string[] Messages)
        {
            Client.LocalUser.SendMessage(PlayerName, Messages[RNG.Next(Messages.Length)]);
        }
    }
}
