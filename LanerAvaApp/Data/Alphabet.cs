using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class Alphabet
{
    public int IdAlphabet { get; set; }

    public string Name { get; set; } = null!;

    public int LangId { get; set; }

    public virtual Language Lang { get; set; } = null!;

    public virtual ICollection<Symbol> Symbols { get; set; } = new List<Symbol>();
}
