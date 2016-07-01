using DnDBot.Data;
using System.Linq;

namespace DnDBot
{
    public partial class Program
    {

        NotesDataDataContext NotesDB = new NotesDataDataContext();
        void Note(string NoteKey, string NoteData)
        {
            Note Note;
            if (NotesDB.Notes.Count() > 0 && (Note = NotesDB.Notes.FirstOrDefault(x => x.NoteKey.ToLower() == NoteKey.ToLower())) != null)
            {
                Note.NoteData = NoteData;
            }
            else if (string.IsNullOrWhiteSpace(NoteKey) || string.IsNullOrWhiteSpace(NoteData))
            {
                SayInChannel(CantDoThat);
            }
            else
            {
                Note NewNote = new Note()
                {
                    NoteKey = NoteKey,
                    NoteData = NoteData
                };
                NotesDB.Notes.InsertOnSubmit(NewNote);
            }
            NotesDB.SubmitChanges();
            SayInChannel(InfoSaved);
        }

        void Recall(string NoteKey)
        {
            var Note = NotesDB.Notes.FirstOrDefault(x => x.NoteKey.ToLower() == NoteKey.ToLower());

            SayInChannel(Searching);
            if (Note != null)
            {
                SayInChannel(NoteKey + ": " + Note.NoteData);
            }
            else
            {
                SayInChannel(SearchFail);
            }
        }
    }
}
