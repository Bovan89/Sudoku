using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Sudoku
{
    public class SudokuGrid
    {
        private int Size;
        private int Size2;
        private int FillCnt;

        public int[,] Grid { get; private set; }
        public bool IsFinish { get { return FillCnt == Size2 * Size2; } }

        private delegate void GridTransform();        

        ///

        public SudokuGrid(int size = 3, int difficult = 3)
        {
            Size = size;
            Size2 = Size * Size;

            Reset();
            Mix();
            Prepare(difficult);
        }

        public void Reset()
        {
            Grid = new int[Size2, Size2];

            for (int i = 0; i < Size2; i++)
            {
                for (int j = 0; j < Size2; j++)
                {
                    Grid[i, j] = ((i * Size + i / Size + j) % (Size2) + 1);
                }                    
            }

            FillCnt = Size2 * Size2;
        }

        public void Mix()
        {
            Random rnd = new Random();
            GridTransform[] trans = new GridTransform[5] { Transpose, SwapRows, SwapColumns, SwapRowsArea, SwapColumnsArea };

            GridTransform doTrans = Reset;
            int transformCnt = rnd.Next(int.MaxValue) % 30;
            for (int i = 0; i < transformCnt; i++)
            {
                doTrans += trans[rnd.Next(int.MaxValue) % 5];
            }

            doTrans();
        }

        public bool Check()
        {
            for (int i = 0; i < Size2; i++)
            {
                if (!CheckRow(i))
                {
                    return false;
                }

                if (!CheckColumn(i))
                {
                    return false;
                }
            }

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (!CheckArea(i, j))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Prepare(int difficult)
        {
            int x, y;
            Random rnd = new Random();

            int clearCnt = Size2 * difficult;

            for (int i = 0; i < clearCnt; i++)
            {
                do
                {
                    x = rnd.Next(int.MaxValue) % Size2;
                    y = rnd.Next(int.MaxValue) % Size2;

                } while (Grid[x, y] == 0);

                Grid[x, y] = 0;
                FillCnt--;
            }
        }

        public Boolean FillValue(int x, int y, int value)
        {
            if (Grid != null)
            {
                if (Grid[x, y] != 0)
                {
                    return false;
                }

                Grid[x, y] = value;

                if (!Check())
                {
                    Grid[x, y] = 0;

                    return false;
                }
                else
                {
                    FillCnt++;
                    return true;
                }
            }
            
            return false;
        }
        
        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();

            for (int i = 0; i < Size2; i++)
            {
                for (int j = 0; j < Size2; j++)
                {
                    ret.Append(Grid[i, j]);
                    ret.Append(':');
                }
            }

            if (ret.Length > 0)
            {
                ret.Remove(ret.Length - 1, 1);
            }

            return ret.ToString();
        }

        ///

        protected bool CheckRow(int rowId)
        {
            List<int> values = new List<int>();

            for (int j = 0; j < Size2; j++)
            {
                if (Grid[rowId, j] != 0)
                {
                    if (values.Contains(Grid[rowId, j]))
                    {
                        return false;
                    }
                    values.Add(Grid[rowId, j]);
                }
            }

            return true;
        }

        protected bool CheckColumn(int columnId)
        {
            List<int> values = new List<int>();

            for (int i = 0; i < Size2; i++)
            {
                if (Grid[i, columnId] != 0)
                {
                    if (values.Contains(Grid[i, columnId]))
                    {
                        return false;
                    }
                    values.Add(Grid[i, columnId]);
                }
            }

            return true;
        }

        protected bool CheckArea(int x, int y)
        {
            List<int> values = new List<int>();
            int value;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    value = Grid[x * Size + i, y * Size + j];
                    if (value != 0)
                    {
                        if (values.Contains(value))
                        {
                            return false;
                        }
                        values.Add(value);
                    }
                }
            }

            return true;
        }

        protected void Transpose()
        {
            int buf;
            for (int i = 1; i < Size2; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    buf = Grid[i, j];
                    Grid[i, j] = Grid[j, i];
                    Grid[j, i] = buf;                    
                }
            }
        }

        protected void SwapRows()
        {
            Random rnd = new Random();
            int buf;

            int areaId = rnd.Next(Size);
            int firstRowId = areaId * Size + rnd.Next(Size);
            int secondRowId = areaId * Size + rnd.Next(Size);

            while (firstRowId == secondRowId)
            {
                secondRowId = areaId * Size + rnd.Next(Size);
            }

            for (int j = 0; j < Size2; j++)
            {
                buf = Grid[firstRowId, j];
                Grid[firstRowId, j] = Grid[secondRowId, j];
                Grid[secondRowId, j] = buf;
            }
        }

        protected void SwapColumns()
        {
            Random rnd = new Random();
            int buf;

            int areaId = rnd.Next(Size);
            int firstColumnId = areaId * Size + rnd.Next(Size);
            int secondColumnId = areaId * Size + rnd.Next(Size);

            while (firstColumnId == secondColumnId)
            {
                secondColumnId = areaId * Size + rnd.Next(Size);
            }

            for (int i = 0; i < Size2; i++)
            {
                buf = Grid[i, firstColumnId];
                Grid[i, firstColumnId] = Grid[i, secondColumnId];
                Grid[i, secondColumnId] = buf;
            }
        }

        protected void SwapRowsArea()
        {
            Random rnd = new Random();
            int buf;

            int firstAreaId = rnd.Next(int.MaxValue) % 3;
            int secondAreaId = rnd.Next(int.MaxValue) % 3;
            while (firstAreaId == secondAreaId)
            {
                secondAreaId = rnd.Next(int.MaxValue) % 3;
            }
            firstAreaId *= Size;
            secondAreaId *= Size;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size2; j++)
                {
                    buf = Grid[i + firstAreaId, j];
                    Grid[i + firstAreaId, j] = Grid[i + secondAreaId, j];
                    Grid[i + secondAreaId, j] = buf;
                }
            }
        }

        protected void SwapColumnsArea()
        {
            Random rnd = new Random();
            int buf;

            int firstAreaId = rnd.Next(int.MaxValue) % 3;
            int secondAreaId = rnd.Next(int.MaxValue) % 3;
            while (firstAreaId == secondAreaId)
            {
                secondAreaId = rnd.Next(int.MaxValue) % 3;
            }
            firstAreaId *= Size;
            secondAreaId *= Size;

            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size2; i++)
                {
                    buf = Grid[i, j + firstAreaId];
                    Grid[i, j + firstAreaId] = Grid[i, j + secondAreaId];
                    Grid[i, j + secondAreaId] = buf;
                }
            }
        }
    }
}