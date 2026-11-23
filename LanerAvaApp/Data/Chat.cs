using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class Chat
{
    public int IdChat { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User { get; set; } = null!;
}
