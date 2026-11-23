using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class ChatListItem : INotifyPropertyChanged
    {
        public int IdChat { get; set; }
        public int UserId { get; set; }

        private string _fullname { get; set; } = "";
        public string Fullname
        {
            get => _fullname;
            set { if (_fullname != value) { _fullname = value; OnPropertyChanged(); } }
        }

        private string? _lastMessageText { get; set; } = "";
        public string? LastMessageText
        {
            get => _lastMessageText;
            set { if (_lastMessageText != value) { _lastMessageText = value; OnPropertyChanged(); } }
        }

        private DateTime? _lastMessageAt { get; set; } = DateTime.Now;
        public DateTime? LastMessageAt
        {
            get => _lastMessageAt;
            set { if (_lastMessageAt != value) { _lastMessageAt = value; OnPropertyChanged(); } }
        }

        public string TimeText => LastMessageAt?.ToString("HH:mm") ?? "";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
