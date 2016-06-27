using DnDBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DnDBot
{
    public partial class Program
    {
        EventsDataContext EventsDB = new EventsDataContext();

        void Log(string Event, string Text)
        {

            string[] HashTags = HashExtractor.Matches(Text).Cast<Match>().Select(x => x.Groups[1].Value).ToArray();

            if (HashTags.Length < 1)
            {
                SayInChannel(CantDoThat);
                return;
            }

            if (string.IsNullOrWhiteSpace(Event))
            {
                SayInChannel(CantDoThat);
                return;
            }

            Event LogEvent = new Event();
            LogEvent.NoteData = Event;
            LogEvent.Timestamp = DateTime.Now;
            EventsDB.Events.InsertOnSubmit(LogEvent);
            EventsDB.SubmitChanges();

            foreach (string Hash in HashTags)
                EventsDB.EventHashtags.InsertOnSubmit(new Data.EventHashtag() { EventId = LogEvent.Id, TagId = HashTag(Hash) });

            EventsDB.SubmitChanges();
        }

        int HashTag(string HashTag)
        {
            HashTag Id = EventsDB.HashTags.FirstOrDefault(x => x.Tag.ToLower() == HashTag.ToLower());
            if (Id == null)
                EventsDB.HashTags.InsertOnSubmit(Id = new HashTag() { Tag = HashTag });

            EventsDB.SubmitChanges();
            return Id.Id;
        }

        static Regex HashExtractor = new Regex(@"\#(\w+)", RegexOptions.Compiled);
        void SearchByHashTags(string Text, string PageString)
        {
            string[] HashTags = HashExtractor.Matches(Text).Cast<Match>().Select(x => x.Groups[1].Value).ToArray();

            if (HashTags.Length < 1)
            {
                SayInChannel(CantDoThat);
                return;
            }

            var HashTagIDs = (from HT in EventsDB.HashTags where HashTags.Contains(HT.Tag) select HT.Id).ToArray();

            int Page = 0;
            if (!int.TryParse(PageString, out Page))
                Page = 1;

            if (Page < 0)
            {
                SayInChannel(CantDoThat);
                return;
            }

            var Events = (from EV in EventsDB.Events
                          join HT in EventsDB.EventHashtags on EV equals HT.Event
                          where HashTagIDs.Contains(HT.TagId)
                          select EV)
                          .GroupBy(g => g.Id)
                          .Where(x => x.Count() == HashTags.Length)
                          .Select(x => x.First())
                          .OrderByDescending(x => x.Timestamp)
                          .Skip((Page - 1) * 10)
                          .Take(10);

            SayInChannel(Searching);
            if (Events.Any(x => true))
            {
                SayInChannel("3 ");
                foreach (var Event in Events)
                {
                    SayInChannel(" * " + Event.NoteData);
                }
                SayInChannel("3 ");
            }
            else
            {
                SayInChannel(SearchFail);
            }

        }

        const int Width = 120;
        void SendHashTags(string Requester)
        {
            int MaxLen = (from H in EventsDB.HashTags select H.Tag.Length).Max() + 3;
            int Num = Width / MaxLen;

            List<string> Lines = new List<string>();
            string S = "";
            int Count = 0;
            foreach (HashTag Tag in EventsDB.HashTags)
            {
                S = S + "#" + Tag.Tag + "  ";
                Count++;
                if (Count % Num == 0)
                {
                    Lines.Add(S);
                    S = "";
                }
            }
            if (!string.IsNullOrWhiteSpace(S))
                Lines.Add(S);

            MessagePlayer(Requester, ListHashtags);
            MessagePlayer(Requester, new string('-', 120));

            foreach (var Line in Lines)
                MessagePlayer(Requester, Line);

        }
    }
}
