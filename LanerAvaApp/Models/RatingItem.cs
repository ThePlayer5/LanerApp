using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class RatingItem
    {
        public int Rank { get; set; }
        public string Fullname { get; set; } = "";
        public string ValueText { get; set; } = "";

        public string RankText => $"{Rank}";
        public bool IsFirst => Rank == 1;
        public bool IsSecond => Rank == 2;
        public bool IsThird => Rank == 3;
    }
}
