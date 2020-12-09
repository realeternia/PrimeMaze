namespace mazeGenerator_dfs
{
    public struct Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsCell { get; set; }
        public bool IsVisited { get; set; }

        public Cell(int x, int y, bool isVisited = false, bool isCell = true)
        {
            X = x < 0 ? 0 : x;
            Y = y < 0 ? 0 : y;
            IsCell = isCell;
            IsVisited = isVisited;
        }
    }
}

