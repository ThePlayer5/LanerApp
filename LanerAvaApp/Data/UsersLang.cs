using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class UsersLang
{
    public int IdUserLang { get; set; }

    public int UserId { get; set; }

    public int LangId { get; set; }

    public virtual Language Lang { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
