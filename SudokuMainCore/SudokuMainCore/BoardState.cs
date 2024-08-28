using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuMainCore
{
    public class BoardState
    {
        public List<List<int>> Board { get; set; }
        public string[,] UserValues { get; set; }
        public List<Tuple<int, int>> HiddenCells { get; set; }
        public int Attempt { get; set; }

    }

}
