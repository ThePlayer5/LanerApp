using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class LearnedWord
{
    public int IdLearnWord { get; set; }

    public DateTime Timing { get; set; }

    public int WordId { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Word Word { get; set; } = null!;
}
