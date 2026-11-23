using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class Symbol
{
    public int IdSymbol { get; set; }

    public string Symbol1 { get; set; } = null!;

    public int AlphabetId { get; set; }

    public string Transliteration { get; set; } = null!;

    public virtual Alphabet Alphabet { get; set; } = null!;
}
