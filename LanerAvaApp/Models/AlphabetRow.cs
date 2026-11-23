using LanerAvaApp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class AlphabetRow : INotifyPropertyChanged
    {
        private readonly Alphabet _model;

        public AlphabetRow(Alphabet alphabet) => _model = alphabet;
        public AlphabetRow() => _model = new Alphabet { Name = "" };

        public Alphabet Model => _model;

        public int IdAlphabet
        {
            get => _model.IdAlphabet;
            set { _model.IdAlphabet = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _model.Name ?? "";
            set { _model.Name = value; OnPropertyChanged(); }
        }

        public int LangId
        {
            get => _model.LangId;
            set { _model.LangId = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
