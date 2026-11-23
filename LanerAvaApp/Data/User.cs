using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class User
{
    public int IdUser { get; set; }

    public string Fullname { get; set; } = null!;

    public string Ulogin { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

    public virtual ICollection<LearnedWord> LearnedWords { get; set; } = new List<LearnedWord>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UsersLang> UsersLangs { get; set; } = new List<UsersLang>();
}
