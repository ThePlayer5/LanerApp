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
    public class LanguageRow : INotifyPropertyChanged
    {
        private readonly Language _model;

        public LanguageRow(Language model) => _model = model;
        public LanguageRow() => _model = new Language { Name = "" };

        public Language Model => _model;

        public int IdLang
        {
            get => _model.IdLang;
            set { _model.IdLang = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _model.Name ?? "";
            set { _model.Name = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
