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
    public class WordRow : INotifyPropertyChanged
    {
        private readonly Word _model;

        public WordRow(Word model) => _model = model;
        public WordRow() => _model = new Word { Word1 = "", Translation = "", Transcription = "" };

        public Word Model => _model;

        public int IdWord
        {
            get => _model.IdWord;
            set { _model.IdWord = value; OnPropertyChanged(); }
        }

        public string Word
        {
            get => _model.Word1 ?? "";
            set { _model.Word1 = value; OnPropertyChanged(); }
        }

        public string Translation
        {
            get => _model.Translation ?? "";
            set { _model.Translation = value; OnPropertyChanged(); }
        }

        public string Transcription
        {
            get => _model.Transcription ?? "";
            set { _model.Transcription = value; OnPropertyChanged(); }
        }

        public int LangId
        {
            get => _model.LangId;
            set { _model.LangId = value; OnPropertyChanged(); }
        }

        public int LevelId
        {
            get => _model.LevelId;
            set { _model.LevelId = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
