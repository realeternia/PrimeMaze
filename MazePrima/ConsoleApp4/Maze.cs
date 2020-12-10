using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using mazeGenerator_dfs;

namespace ConsoleApp4
{
    internal class Maze
    {
        public readonly Cell[,] Cells;
        public int Width;
        public int Height;
        public Stack<Cell> _path = new Stack<Cell>();
        public List<Cell> _solve = new List<Cell>();
        public List<Cell> _visited = new List<Cell>();
        public List<Cell> _neighbours = new List<Cell>();
        public Random rnd = new Random();
        public Cell Start;
        public Cell Finish;
        public Cell End;

        public static Result<Maze> Create(int width, int height)
        {
            var errors = new List<string>();

            if (width < 5) { errors.Add("Параметр width должен быть больше 4"); };
            if (height < 5) { errors.Add("Параметр height должен быть больше 4"); }

            if (errors.Any())
            {
                return Result<Maze>.Fail(errors);
            }

            return Result<Maze>.Success(new Maze(width, height));
        }


        private Maze(int width, int height)
        {
            Width = width;
            Height = height;

            var createResult = Cell.Create(1, 1, true, true);
            if (!createResult.Succeeded)
            {
                MessageBox.Show(string.Join(Environment.NewLine, createResult.Errors), "Ошибка создания ячейки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Start = createResult.Value;

            createResult = Cell.Create(Width - 3, Height - 3, true, true);
            if (!createResult.Succeeded)
            {
                MessageBox.Show(string.Join(Environment.NewLine, createResult.Errors), "Ошибка создания ячейки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Finish = createResult.Value;

            Cells = new Cell[Width, Height];
        }

        public void MazeInit()
        {
            for (var i = 0; i < Width; i++)
                for (var j = 0; j < Height; j++)
                    if ((i % 2 != 0 && j % 2 != 0) && (i < this.Width - 1 && j < this.Height - 1)) //если ячейка нечетная по х и по у и не выходит за пределы лабиринта
                    {
                        var createResult = Cell.Create(i, j);
                        if (!createResult.Succeeded)
                        {
                            MessageBox.Show(string.Join(Environment.NewLine, createResult.Errors), "Ошибка создания ячейки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Cells[i, j] = createResult.Value; //то это клетка (по умолчанию)
                    }
                    else
                    {
                        var createResult = Cell.Create(i, j, false, false);
                        if (!createResult.Succeeded)
                        {
                            MessageBox.Show(string.Join(Environment.NewLine, createResult.Errors), "Ошибка создания ячейки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        Cells[i, j] = createResult.Value;
                    }
            _path.Push(Start);
            Cells[Start.X, Start.Y] = Start;
        }

        public void MazeCreate()
        {
            Cells[Start.X, Start.Y] = Start;
            while (_path.Count != 0) //пока в стеке есть клетки ищем соседей и строим путь
            {
                _neighbours.Clear();
                GetNeighbours(_path.Peek());
                if (_neighbours.Count != 0)
                {
                    Cell nextCell = ChooseNeighbour(_neighbours);
                    RemoveWall(_path.Peek(), nextCell);
                    nextCell.IsVisited = true; //делаем текущую клетку посещенной
                    Cells[nextCell.X, nextCell.Y].IsVisited = true; //и в общем массиве
                    _path.Push(nextCell); //затем добавляем её в стек

                }
                else
                {
                    _path.Pop();
                }
            }

            var endCels = new List<Cell>();

            foreach (var cell in Cells)
            {
                int x = cell.X;
                int y = cell.Y;

                if (!Cells[x, y].IsCell)
                {
                    continue;
                }

                const int distance = 1;
                List<Cell> possibleNeighbours;

                try
                {
                    possibleNeighbours = new List<Cell>
                    {
                        Cells[x, y - distance], Cells[x + distance, y], Cells[x, y + distance], Cells[x - distance, y]
                    };

                }
                catch (Exception)
                {
                    continue;
                }
                
                var wayCount = 0;
                foreach (var neighbour in possibleNeighbours)
                {
                    if (neighbour.IsCell)
                    {
                        wayCount++;
                    }
                }

                if (wayCount == 0) continue;
                if (wayCount == 1) endCels.Add(Cells[cell.X, cell.Y]);
            }

            End = endCels.LastOrDefault();


        }

        private void GetNeighbours(Cell localcell) // Получаем соседа текущей клетки
        {
            int x = localcell.X;
            int y = localcell.Y;
            const int distance = 2;

            var distX = x - distance < 0 ? 0 : x - distance;
            var distY = y - distance < 0 ? 0 : y - distance;
            var createResult = Cell.Create(x, distY);
            if (!createResult.Succeeded)
            {
                MessageBox.Show(string.Join(Environment.NewLine, createResult.Errors), "Ошибка создания ячейки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var up = createResult.Value;

            createResult = Cell.Create(x + distance, y);
            if (!createResult.Succeeded)
            {
                MessageBox.Show(string.Join(Environment.NewLine, createResult.Errors), "Ошибка создания ячейки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var right = createResult.Value;

            createResult = Cell.Create(x, y + distance);
            if (!createResult.Succeeded)
            {
                MessageBox.Show(string.Join(Environment.NewLine, createResult.Errors), "Ошибка создания ячейки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var down = createResult.Value;

            createResult = Cell.Create(distX, y);
            if (!createResult.Succeeded)
            {
                MessageBox.Show(string.Join(Environment.NewLine, createResult.Errors), "Ошибка создания ячейки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var left = createResult.Value;

            Cell[] possibleNeighbours = new[] // Список всех возможных соседeй
            {
                up, // Up
                right, // Right
                down, // Down
                left // Left
            };
            for (int i = 0; i < 4; i++) // Проверяем все 4 направления
            {
                Cell curNeighbour = possibleNeighbours[i];
                if (curNeighbour.X > 0 && curNeighbour.X < Width && curNeighbour.Y > 0 && curNeighbour.Y < Height)
                {// Если сосед не выходит за стенки лабиринта
                    if (Cells[curNeighbour.X, curNeighbour.Y].IsCell && !Cells[curNeighbour.X, curNeighbour.Y].IsVisited)
                    { // А также является клеткой и непосещен
                        _neighbours.Add(curNeighbour);
                    }// добавляем соседа в Лист соседей
                }
            }
        }

        
        private Cell ChooseNeighbour(List<Cell> neighbours) //выбор случайного соседа
        {

            int r = rnd.Next(neighbours.Count);
            return neighbours[r];

        }

        private void RemoveWall(Cell first, Cell second)
        {
            int xDiff = second.X - first.X;
            int yDiff = second.Y - first.Y;
            int addX = (xDiff != 0) ? xDiff / Math.Abs(xDiff) : 0; // Узнаем направление удаления стены
            int addY = (yDiff != 0) ? yDiff / Math.Abs(yDiff) : 0;

            // Координаты удаленной стены
            Cells[first.X + addX, first.Y + addY].IsCell = true; //обращаем стену в клетку
            Cells[first.X + addX, first.Y + addY].IsVisited = true; //и делаем ее посещенной
            second.IsVisited = true; //делаем клетку посещенной
            Cells[second.X, second.Y] = second;

        }
    }
}
