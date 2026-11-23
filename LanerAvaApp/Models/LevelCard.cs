using Avalonia.Media;
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
    public class LevelCard : INotifyPropertyChanged
    {
        private int _wordsQty;

        public int LevelId { get; set; }
        public string LevelName { get; set; } = "";
        public IBrush CardBackground { get; set; } = Brushes.White;

        public int WordsQty
        {
            get => _wordsQty;
            set
            {
                if (value <= 0) return;
                if (_wordsQty == value) return;
                _wordsQty = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
