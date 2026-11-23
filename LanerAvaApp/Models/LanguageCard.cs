using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class LanguageCard : INotifyPropertyChanged
    {
        private decimal _progress;

        public int LangId { get; set; }
        public string LanguageName { get; set; } = "";
        public Bitmap? ImagePath { get; set; }

        public decimal Progress
        {
            get => _progress;
            set
            {
                if (Math.Abs(_progress - value) < 0.001m) return;
                _progress = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
