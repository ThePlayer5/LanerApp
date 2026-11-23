using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class WordCard
    {
        public int IdWord { get; set; }

        public string Word1 { get; set; } = "";

        public string Translation { get; set; } = "";

        public string Transcription { get; set; } = "";

        public int LangId { get; set; }

        public int LevelId { get; set; }

        public string Section { get; set; } = "";

        public bool IsLearned { get; set; }
    }
}
