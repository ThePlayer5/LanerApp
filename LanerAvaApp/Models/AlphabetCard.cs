using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class AlphabetCard
    {
        public int IdAlphabet { get; set; }
        public string Name { get; set; } = "";
        public int LangId { get; set; }
        public string LangName { get; set; }
        public IBrush CardBackground { get; set; } = Brushes.White;

    }
}
