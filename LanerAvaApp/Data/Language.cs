using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class Language
{
    public int IdLang { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Alphabet> Alphabets { get; set; } = new List<Alphabet>();

    public virtual ICollection<UsersLang> UsersLangs { get; set; } = new List<UsersLang>();

    public virtual ICollection<Word> Words { get; set; } = new List<Word>();
}
