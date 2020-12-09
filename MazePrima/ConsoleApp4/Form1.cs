using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mazeGenerator_dfs;

namespace ConsoleApp4 {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }


        private int CellWid, CellHgt;
        Maze inMaze = new Maze(10, 10);
        Bitmap inBm = new Bitmap(1, 1);


        private void saveBtn_Click(object sender, EventArgs e) {
            SaveFileDialog savedialog = new SaveFileDialog();
            savedialog.Title = "Сохранить картинку как...";
            //отображать ли предупреждение, если пользователь указывает имя уже существующего файла
            savedialog.OverwritePrompt = true;
            //отображать ли предупреждение, если пользователь указывает несуществующий путь
            savedialog.CheckPathExists = true;
            //список форматов файла, отображаемый в поле "Тип файла"
            savedialog.Filter = "Image Files(*.JPG)|*.JPG|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
            //отображается ли кнопка "Справка" в диалоговом окне
            savedialog.ShowHelp = true;
            if (savedialog.ShowDialog() == DialogResult.OK) //если в диалоговом окне нажата кнопка "ОК"
            {
                try {
                    picMaze.Image.Save(savedialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                } catch {
                    MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static int GetHeight() {
            var errors = new List<string>();

            if (!int.TryParse(ConfigurationManager.AppSettings["Height"], out var height))
            {
                errors.Add("Поле Height не удалось преобразовать в int");
            }

            if (height < 5)
            {
                errors.Add("Поле Height не может иметь значение меньше 5");
            }

            return errors.Any() ? 10 : height;
        }
        public static int GetWidth()
        {
            var errors = new List<string>();

            if (!int.TryParse(ConfigurationManager.AppSettings["Width"], out var width))
            {
                errors.Add("Поле Width не удалось преобразовать в int");
            }

            if (width < 5)
            {
                errors.Add("Поле Width не может иметь значение меньше 5");
            }
            
            return errors.Any() ? 10 : width;
        }

        private void Form1_Load(object sender, EventArgs e) {
            var heigthFromConfig = GetHeight();
            var widthFromConfig = GetWidth();
            txtHeight.Text = heigthFromConfig.ToString();
            txtWidth.Text = widthFromConfig.ToString();
        }

        private void createBtn_Click(object sender, EventArgs e)
        {
            var errors = new List<string>();

            //Добавим проверку на корректность введенных размеров
            if (!int.TryParse(txtWidth.Text, out var wid) || wid < 5)
            {
                errors.Add("Поле Width должно иметь значение больше 4");
            }
            if (!int.TryParse(txtHeight.Text, out var hgt) || hgt < 5)
            {
                errors.Add("Поле Height должно иметь значение больше 4");
            }

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join(", ", errors), "Error", MessageBoxButtons.OK);
                return;
            }
            
            int oddW = 0;
            int oddH = 0;

            //Обрабатываем случай с нечетными размерами
            if (wid % 2 != 0 && wid != 0) {
                oddW = 1;
            }
            if (hgt % 2 != 0 && hgt != 0) {
                oddH = 1;
            }

            //вычисляем ширину одной ячейки, чтобы автомасштабировать полученную картинку

            CellWid = picMaze.ClientSize.Width / (wid + 2);
            CellHgt = picMaze.ClientSize.Height / (hgt + 2);

            //Установим минимальный размер ячейки, чтобы глаза не выпадывали
            int CellMin = 10;
            if (CellWid < CellMin) {
                CellWid = CellMin;
                CellHgt = CellWid;
            } else if (CellHgt < CellMin) {
                CellHgt = CellMin;
                CellWid = CellHgt;
            } else if (CellWid > CellHgt) CellWid = CellHgt;
            else CellHgt = CellWid;

            
            Maze maze = new Maze(wid, hgt);
            maze.MazeInit();

            //обрабатываем прорисовку финиша при нечетных размерах
            maze.Finish.X = maze.Finish.X + oddW;
            maze.Finish.Y = maze.Finish.Y + oddH;
            maze.MazeCreate();
            DrawMaze();

            inMaze = maze;

            void DrawMaze() {
                inBm.Dispose();
                //создаем битмап так, чтобы захватить и финиш и стенку за ним
                Bitmap bm = new Bitmap(
                    CellWid * (maze.Finish.X + 2),
                    CellHgt * (maze.Finish.Y + 2), System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

                Brush whiteBrush = new SolidBrush(Color.White);
                Brush blackBrush = new SolidBrush(Color.Black);

                using (Graphics gr = Graphics.FromImage(bm)) {

                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    for (var i = 0; i < maze.Cells.GetUpperBound(0) + oddW; i++)
                        for (var j = 0; j < maze.Cells.GetUpperBound(1) + oddH; j++) {
                            Point point = new Point(i * CellWid, j * CellWid);
                            Size size = new Size(CellWid, CellWid);
                            Rectangle rec = new Rectangle(point, size);
                            if (maze.Cells[i, j].IsCell) {
                                gr.FillRectangle(whiteBrush, rec);
                            } else {

                                gr.FillRectangle(blackBrush, rec);
                            }
                        }

                    gr.FillRectangle(new SolidBrush(Color.Green),    //заливаем старт зеленым
                        new Rectangle(new Point(maze.Start.X * CellWid, maze.Start.Y * CellWid),
                        new Size(CellWid, CellWid)));
                    gr.FillRectangle(new SolidBrush(Color.Red),       //а финиш красным
                        new Rectangle(new Point(maze.End.X * CellWid, maze.End.Y * CellWid),
                        new Size(CellWid, CellWid)));
                }

                picMaze.Image = bm; //отображаем 
                inBm = bm;

            }
        }
    }
}
