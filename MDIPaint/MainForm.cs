using MyNewPaint;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDIPaint
{
    public partial class MainForm : Form
    {
        // Автосвойства
        public static int CurrentWidth { get; set; }
        public static Color CurrentColor {  get; set; }
        public static Tools CurrentTool { get; set; }
        public static bool FillShapes { get; set; } = false;


        // Конструктор
        public MainForm()
        {
            InitializeComponent();

            CurrentColor = Color.Black;
            CurrentWidth = 1;
            CurrentTool = Tools.Pen;

            FillShapes = false;
            UpdateToolStripButton5();
        }



        // ToolStripMenu1
        // Файл
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = new FormDocument();
            doc.MdiParent = this;
            doc.Show();
        }

        private void создатьСПараметрамиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var canvas = new FormCanvasSize();

            if (canvas.ShowDialog() == DialogResult.OK)
            {
                int width = canvas.NewWidth;
                int height = canvas.NewHeight;

                Bitmap newBmp = new Bitmap(width, height);

                var doc = new FormDocument(newBmp);
                doc.MdiParent = this;
                doc.Show();
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "BMP|*.bmp|JPG|*.jpg;*.jpeg";
                dlg.DefaultExt = "bmp";
                dlg.Multiselect = false;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var newDocument = new FormDocument();
                    newDocument.LoadImage(dlg.FileName);
                    newDocument.MdiParent = this;
                    newDocument.Show();
                }
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = ActiveMdiChild as FormDocument;
            if (f != null)
                f.Save();
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = ActiveMdiChild as FormDocument;

            if (f != null)
            {
                var dlg = new SaveFileDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                    f.SaveAs(dlg.FileName);
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Application.Exit();
        }


        // Рисунок
        private void размерХолстаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var canvas = new FormCanvasSize();
            if (canvas.ShowDialog() == DialogResult.OK)
            {
                if (ActiveMdiChild == null)
                {
                    createDocWithParam(canvas.NewWidth, canvas.NewHeight);
                }
                else
                {
                    var amd = ActiveMdiChild as FormDocument;
                    if (amd != null)
                    {
                        try
                        {
                            int newWidth = canvas.NewWidth;
                            int newHeight = canvas.NewHeight;

                            Bitmap newBmp = new Bitmap(newWidth, newHeight);

                            using (Graphics g = Graphics.FromImage(newBmp))
                            {
                                g.Clear(Color.White);
                                g.DrawImage(amd.bmp, 0, 0);
                            }

                            amd.bmp = newBmp;
                            amd.bmpTemp = newBmp;
                            amd.Invalidate();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
        private void рисунокToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            размерХолстаToolStripMenuItem.Enabled = !(ActiveMdiChild == null);
        }
        private void createDocWithParam(int width, int height)
        {
            Bitmap newBmp = new Bitmap(width, height);

            var doc = new FormDocument(newBmp);
            doc.MdiParent = this;
            doc.Show();
        }


        // Окно
        private void каскадомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }
        private void слеваНаправоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void сверхуВнизToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void упорядочитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }


        // Помощь
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new FormAbout();
            frm.ShowDialog();
        }




        // ToolStripMenu2
        public static Cursor GetCursorForTool(Tools tool)
        {
            switch (tool)
            {
                case Tools.Bucket:
                    return Cursors.Hand;
                case Tools.Eraser:
                    return Cursors.No;
                case Tools.Pen:
                    return Cursors.Cross;
                case Tools.Line:
                    return Cursors.Cross;
                case Tools.Circle:
                    return Cursors.UpArrow;
                case Tools.Rectangle:
                    return Cursors.UpArrow;
                case Tools.Heart:
                    return Cursors.PanSouth;
                default:
                    return Cursors.Default;
            }
        }
        // Смена цвета
        private void красныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentColor = Color.Red;
        }

        private void синийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentColor = Color.Blue;
        }

        private void зелёныйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentColor = Color.Green;
        }

        private void другойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new ColorDialog(); 
            if (dlg.ShowDialog() == DialogResult.OK)
                CurrentColor = dlg.Color;
        }


        // Настройка толщины
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            CurrentWidth = 1;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            CurrentWidth = 3;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            CurrentWidth = 5;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            CurrentWidth = 10;
        }

        // Инструменты
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Text;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Bucket;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Eraser;
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Pen;
        }


        // Фигуры
        private void линияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Line;
        }

        private void эллипсToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Circle;
        }

        private void прямоугольникToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Rectangle;
        }

        private void сердцеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTool = Tools.Heart;
        }

        // Закрашена фигура или нет
        private void заливкаВыклToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FillShapes = !FillShapes;
            UpdateToolStripButton5();
        }
        private void UpdateToolStripButton5()
        {
            if (FillShapes)
                заливкаВыклToolStripMenuItem.Text = "Заливка: Вкл";
            else
                заливкаВыклToolStripMenuItem.Text = "Заливка: Выкл";
        }


        // Zoom
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            var a = ActiveMdiChild as FormDocument;
            a?.ZoomIn();
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            var a = ActiveMdiChild as FormDocument;
            a?.ZoomOut();
        }



        // ToolStripMenu3
        public void ShowPosition(int x, int y)
        {
            if (x != -1)
                statusLabelPosition.Text = $"X: {x} Y: {y}";
            else
                statusLabelPosition.Text = string.Empty;
        }
    }
}
