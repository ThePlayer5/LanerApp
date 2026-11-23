using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class GameCard : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        private bool _isFaceUp;
        public bool IsFaceUp
        {
            get => _isFaceUp;
            set
            {
                if (_isFaceUp != value)
                {
                    _isFaceUp = value; OnPropertyChanged();
                }
            }
        }
        private bool _isMatched;
        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                if (_isMatched != value)
                {
                    _isMatched = value; OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
