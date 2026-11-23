using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class MessageRow : INotifyPropertyChanged
    {
        public int IdMessage { get; set; }
        public string MessageText { get; set; } = "";
        public DateTime Timing { get; set; } = DateTime.Now;
        public int? UserId { get; set; }
        public int ChatId { get; set; }

        private int _currentUserId;
        public int CurrentUserId
        {
            get => _currentUserId;
            set
            {
                if (_currentUserId != value)
                {
                    _currentUserId = value;
                    OnPropertyChanged(nameof(IsOwn));
                }
            }
        }

        public bool IsOwn => UserId == CurrentUserId;

        public string TimeText => Timing.ToString("HH:mm");

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
