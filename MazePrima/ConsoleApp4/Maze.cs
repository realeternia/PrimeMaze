using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Specialized;
using mazeGenerator_dfs;

namespace ConsoleApp4
{
    class Maze
    {
        public readonly Cell[,] _cells;
        public int _width;
        public int _height;
        public Stack<Cell> _path = new Stack<Cell>();
        public List<Cell> _solve = new List<Cell>();
        public List<Cell> _visited = new List<Cell>();
        public List<Cell> _neighbours = new List<Cell>();
        public Random rnd = new Random();
        public Cell start;
        public Cell finish;
        public Cell end;

        public Maze(int wid, int hgt)
        {
            start = new Cell(1, 1, true, true);
            finish = new Cell(wid - 3, hgt - 3, true, true);


            




            _cells = new Cell[wid, hgt];
            
            _width = wid;
            _height = hgt;
            for (var i = 0; i < wid; i++)
                for (var j = 0; j < hgt; j++)
                    if ((i % 2 != 0 && j % 2 != 0) && (i < _width - 1 && j < _height - 1)) //если ячейка нечетная по х и по у и не выходит за пределы лабиринта
                    {
                        _cells[i, j] = new Cell(i, j); //то это клетка (по умолчанию)
                    }
                    else
                    {

                        _cells[i, j] = new Cell(i, j, false, false);
                    }
            _path.Push(start);
            _cells[start.X, start.Y] = start;
        }
        
        public void CreateMaze()
        {
            _cells[start.X, start.Y] = start;
            while (_path.Count != 0) //пока в стеке есть клетки ищем соседей и строим путь
            {
                _neighbours.Clear();
                GetNeighbours(_path.Peek());
                if (_neighbours.Count != 0)
                {
                    Cell nextCell = ChooseNeighbour(_neighbours);
                    RemoveWall(_path.Peek(), nextCell);
                    nextCell.IsVisited = true; //делаем текущую клетку посещенной
                    _cells[nextCell.X, nextCell.Y].IsVisited = true; //и в общем массиве
                    _path.Push(nextCell); //затем добавляем её в стек

                }
                else
                {
                    _path.Pop();
                }

            }

            var endCels = new List<Cell>();

            foreach (var cell in _cells)
            {
                int x = cell.X;
                int y = cell.Y;

                if (!_cells[x, y].IsCell)
                {
                    continue;
                }

                const int distance = 1;
                List<Cell> possibleNeighbours;

                try
                {
                    possibleNeighbours = new List<Cell>
                    {
                        _cells[x, y - distance], _cells[x + distance, y], _cells[x, y + distance], _cells[x - distance, y]
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
                if (wayCount == 1) endCels.Add(_cells[cell.X, cell.Y]);
            }

            end = endCels.LastOrDefault();


        }

        private void GetNeighbours(Cell localcell) // Получаем соседа текущей клетки
        {
            int x = localcell.X;
            int y = localcell.Y;
            const int distance = 2;
            Cell[] possibleNeighbours = new[] // Список всех возможных соседeй
            {
                new Cell(x, y - distance), // Up
                new Cell(x + distance, y), // Right
                new Cell(x, y + distance), // Down
                new Cell(x - distance, y) // Left
            };
            for (int i = 0; i < 4; i++) // Проверяем все 4 направления
            {
                Cell curNeighbour = possibleNeighbours[i];
                if (curNeighbour.X > 0 && curNeighbour.X < _width && curNeighbour.Y > 0 && curNeighbour.Y < _height)
                {// Если сосед не выходит за стенки лабиринта
                    if (_cells[curNeighbour.X, curNeighbour.Y].IsCell && !_cells[curNeighbour.X, curNeighbour.Y].IsVisited)
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
            _cells[first.X + addX, first.Y + addY].IsCell = true; //обращаем стену в клетку
            _cells[first.X + addX, first.Y + addY].IsVisited = true; //и делаем ее посещенной
            second.IsVisited = true; //делаем клетку посещенной
            _cells[second.X, second.Y] = second;

        }
    }
}
