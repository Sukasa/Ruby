using System.Linq;
using System.Text.RegularExpressions;

namespace DnDBot
{
    public partial class Program
    {

        static Regex SideRegex = new Regex(@"(\w+)\s*(?:,|})", RegexOptions.Compiled);
        static Regex SidesCheck = new Regex(@"\{\s*(?:(\w+)\,?\s*)+\s*\}", RegexOptions.Compiled);

        void DiceRoll(string NumDiceCode, string SidesCode)
        {
            int NumDice;
            int Sides;

            // Can we get the number of dice?
            if (!int.TryParse(NumDiceCode, out NumDice))
            {
                SayInChannel(CantDoThat);
                return;
            }

            if (NumDice > 16)
            {
                SayInChannel(OutOfRange);
                return;
            }
            
            // Are we rolling standard dice?
            if (int.TryParse(SidesCode, out Sides))
            {
                if (Sides == 2)
                {
                    SidesCode = "{Heads,Tails}";
                    goto HeadsTails; // Special case for coins
                }

                if (Sides < 2)
                {
                    SayInChannel(CantDoThat);
                    return;
                }

                if (Sides > 100)
                {
                    SayInChannel(OutOfRange);
                    return;
                }

                if (NumDice == 1)
                {
                    SayInChannel(string.Format(DiceRollText[RNG.Next(DiceRollText.Length)] + " {0}.", RNG.Next(Sides) + 1));

                    return;
                }

                int[] Rolls = new int[NumDice];
                for (int i = 0; i < NumDice; i++)
                    Rolls[i] = RNG.Next(Sides) + 1;

                SayInChannel(string.Format(DiceRollText[RNG.Next(DiceRollText.Length)] + " {0}. ({1})", Rolls.Sum(), string.Join(", ", Rolls)));

                return;
            }

            HeadsTails:

            // Are we rolling a custom die style?
            if (SidesCheck.IsMatch(SidesCode))
            {
                // Roll custom die
                string[] CustomSides = SideRegex.Matches(SidesCode).Cast<Match>().Select(x => x.Groups[1].Value).ToArray();

                string[] Rolls = new string[NumDice];
                for (int i = 0; i < NumDice; i++)
                    Rolls[i] = CustomSides[RNG.Next(CustomSides.Length)];

                SayInChannel(string.Format(DiceRollText[RNG.Next(DiceRollText.Length)] + " {0}.", string.Join(", ", Rolls)));
                return;
            }

            // Nope, no idea what we're rolling.
            SayInChannel(CantDoThat);
        }
    }
}
