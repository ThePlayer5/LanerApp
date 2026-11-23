using System;
using System.Collections.Generic;

namespace LanerAvaApp.Data;

public partial class LearningProgress
{
    public int? UserId { get; set; }

    public string? Language { get; set; }

    public decimal? Percent { get; set; }
}
