using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class Word
{
    public int IdWord { get; set; }

    public string Word1 { get; set; } = null!;

    public string Translation { get; set; } = null!;

    public string Transcription { get; set; } = null!;

    public int LangId { get; set; }

    public int LevelId { get; set; }

    public virtual Language Lang { get; set; } = null!;

    public virtual ICollection<LearnedWord> LearnedWords { get; set; } = new List<LearnedWord>();

    public virtual DifficultyLevel Level { get; set; } = null!;
}
