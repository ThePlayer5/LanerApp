using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class Message
{
    public int IdMessage { get; set; }

    public DateTime Timing { get; set; }

    public string MessageText { get; set; } = null!;

    public int? UserId { get; set; }

    public int ChatId { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual User? User { get; set; }
}
