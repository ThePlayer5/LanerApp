using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class DifficultyLevel
{
    public int IdLevel { get; set; }

    public string Name { get; set; } = null!;

    public int? Difficulty { get; set; }

    public virtual ICollection<Word> Words { get; set; } = new List<Word>();
}
