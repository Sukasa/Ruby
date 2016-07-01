using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DnDBot
{
    public partial class Program
    {

        static Regex SideRegex = new Regex(@"(\w+)\s*(?:,|})", RegexOptions.Compiled);
        static Regex SidesCheck = new Regex(@"\{\s*(?:(\w+)\,?\s*)+\s*\}", RegexOptions.Compiled);
        static Regex ModifierRegex = new Regex(@"((\+|-|\\|\*|top|bottom)\s*(-?\d+(?:\.\d+)?))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // TODO this function seriously needs refactoring at this point - repeated code, super long, etc.

        void DiceRoll(string NumDiceCode, string SidesCode, string Modifier)
        {
            int NumDice;
            int Sides;
            Tuple<string, float>[] Modifiers;

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

                try
                {
                    Modifiers = ModifierRegex.Matches(Modifier).Cast<Match>().Select(x => new Tuple<string, float>(x.Groups[2].Value, float.Parse(x.Groups[3].Value))).ToArray();
                }
                catch
                {
                    SayInChannel(CantDoThat);
                    return;
                }

                if (Sides == 2 && Modifiers.Length == 0)
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
                    float Sum2 = RNG.Next(Sides) + 1;

                    foreach (var Mod in Modifiers)
                    {
                        switch (Mod.Item1)
                        {
                            case "*":
                                Sum2 *= Mod.Item2;
                                break;
                            case "-":
                                Sum2 -= Mod.Item2;
                                break;
                            case "\\":
                            case "/":
                                Sum2 /= Mod.Item2;
                                break;
                            case "+":
                                Sum2 += Mod.Item2;
                                break;
                        }
                    }

                    Sum2 = (float)Math.Floor(Sum2);
                    SayInChannel(string.Format(DiceRollText[RNG.Next(DiceRollText.Length)] + " {0}.", Sum2));

                    return;
                }

                DieRoll[] Rolls = new DieRoll[NumDice];
                for (int i = 0; i < NumDice; i++)
                    Rolls[i] = new DieRoll(RNG.Next(Sides) + 1);

                // TODO Apply modifiers
                if (Modifiers.Length > 0)
                {
                    if (Modifiers[0].Item1.ToLower() == "top")
                    {
                        int ToRemove = NumDice - (int)Modifiers[0].Item2;
                        while (ToRemove >= 1)
                        {
                            Rolls.First(x => x.Valid && x.Value == Rolls.Where(y => y.Valid).Min(y => y.Value)).Valid = false;
                            ToRemove--;
                        }
                    }

                    if (Modifiers[0].Item1.ToLower() == "bottom")
                    {
                        int ToRemove = NumDice - (int)Modifiers[0].Item2;
                        while (ToRemove >= 1)
                        {
                            Rolls.First(x => x.Valid && x.Value == Rolls.Where(y => y.Valid).Max(y => y.Value)).Valid = false;
                            ToRemove--;
                        }
                    }
                }

                float Sum = (int)Rolls.Where(x => x.Valid).Select(x => x.Value).Sum();

                foreach (var Mod in Modifiers)
                {
                    switch (Mod.Item1)
                    {
                        case "*":
                            Sum *= Mod.Item2;
                            break;
                        case "-":
                            Sum -= Mod.Item2;
                            break;
                        case "\\":
                        case "/":
                            Sum /= Mod.Item2;
                            break;
                        case "+":
                            Sum += Mod.Item2;
                            break;
                    }
                }

                Sum = (float)Math.Floor(Sum);

                SayInChannel(string.Format("{0} {1}. ({2})",
                                           DiceRollText[RNG.Next(DiceRollText.Length)],
                                           Sum,
                                           string.Join(", ", Rolls.Where(x => x.Valid).Select(x => x.ToString()))));

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

                SayInChannel(string.Format(DiceRollText[RNG.Next(DiceRollText.Length)] + " {0}.", string.Join(", ", Rolls.Select(x => x))));
                return;
            }

            // Nope, no idea what we're rolling.
            SayInChannel(CantDoThat);
        }

        class DieRoll
        {
            public bool Valid;
            public int Value;

            public DieRoll(int Val)
            {
                Valid = true;
                Value = Val;
            }

            public override string ToString()
            {
                if (!Valid)
                    return "4" + Value.ToString() + "";
                return Value.ToString();
            }
        }
    }
}
