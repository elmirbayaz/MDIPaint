using MyNewPaint;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDIPaint
{
    public partial class FormDocument : Form
    {
        int oldX, oldY;

        public Bitmap bmp = new Bitmap(400, 400);
        public Bitmap bmpTemp = new Bitmap(400, 400);

        private TextBox textBox;

        private float zF = 1.0f;
        private const float zS = 1.2f;

        public string CurrentFilePath { get; private set; } = null;
        private bool IsChanged = false;


        // Конструкторы
        public FormDocument()
        {
            InitializeComponent();
            bmp = new Bitmap(ClientSize.Width, ClientSize.Height);

            bmpTemp = bmp;

            var background = Graphics.FromImage(bmp);
            background.Clear(Color.White);

            this.Text = "Без названия";
        }

        public FormDocument(Bitmap bmp)
        {
            InitializeComponent();
            this.bmp = bmp;
        }

        // Zoom
        private Point GetScaledPoint(int x, int y)
        {
            return new Point((int)(x / zF), (int)(y / zF));
        }
        public void ZoomIn()
        {
            zF *= zS;
            Invalidate();
        }
        public void ZoomOut()
        {
            zF /= zS;
            Invalidate();
        }


        
        private void FormDocument_MouseUp(object sender, MouseEventArgs e)
        {
            switch (MainForm.CurrentTool)
            {
                case Tools.Line:
                case Tools.Circle:
                case Tools.Rectangle:
                case Tools.Heart:
                    bmp = bmpTemp;
                    Invalidate();
                    break;
            }
        }
        private void FormDocument_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (textBox != null)
                {
                    SaveTextToImage();
                    IsChanged = true;
                }

                var scaledPoint = GetScaledPoint(e.X, e.Y);
                oldX = scaledPoint.X;
                oldY = scaledPoint.Y;

                if (MainForm.CurrentTool == Tools.Text)
                    CreateTextBox(oldX, oldY);

                if (MainForm.CurrentTool == Tools.Bucket)
                {
                    Color targetColor = bmp.GetPixel(scaledPoint.X, scaledPoint.Y);
                    FloodFill(scaledPoint, targetColor, MainForm.CurrentColor);
                }
            }
        }
        private void FormDocument_MouseLeave(object sender, EventArgs e)
        {
            var parent = MdiParent as MainForm;
            parent?.ShowPosition(-1, -1);
        }



        // Инструмент "Текст"
        private void CreateTextBox(int x, int y)
        {
            if (textBox != null)
            {
                Controls.Remove(textBox);
                textBox.Dispose();
            }

            int scrnX = (int)(x * zF);
            int scrnY = (int)(y * zF);

            textBox = new TextBox
            {
                Location = new Point(scrnX, scrnY),
                Font = new Font("Arial", (int)(16 * zF)),
                BorderStyle = BorderStyle.FixedSingle,
                Width = (int)(150 * zF)
            };

            textBox.LostFocus += TextBox_LostFocus;
            textBox.KeyDown += TextBox_KeyDown;
            MouseDown += FormDocument_MouseDown;

            Controls.Add(textBox);
            textBox.Focus();
        }
        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            SaveTextToImage();
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveTextToImage();
                e.SuppressKeyPress = true;
            }
        }
        private void RemoveTextBox()
        {
            if (textBox != null)
            {
                textBox.LostFocus -= TextBox_LostFocus;
                textBox.KeyDown -= TextBox_KeyDown;
                MouseDown -= FormDocument_MouseDown;

                Controls.Remove(textBox);
                textBox.Dispose();
                textBox = null;
            }
        }
        private void SaveTextToImage()
        {
            if (textBox == null) 
                return;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                RemoveTextBox();
                return;
            }

            try
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    using (var font = new Font("Arial", (int)(16 * zF)))
                    using (var brush = new SolidBrush(MainForm.CurrentColor))
                    {
                        float textX = textBox.Location.X / zF;
                        float textY = textBox.Location.Y / zF;

                        g.DrawString(textBox.Text, font, brush, new PointF(textX, textY));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении текста: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RemoveTextBox();

            bmpTemp = (Bitmap)bmp.Clone();

            Invalidate();
        }


        // Инструмент "Заливка"
        private void FloodFill(Point pt, Color targetColor, Color replacementColor)
        {
            if (targetColor.A == replacementColor.A && targetColor.B == replacementColor.B && targetColor.G == replacementColor.G)
                return;
            if (bmp.GetPixel(pt.X, pt.Y) != targetColor)
                return;

            Stack<Point> pixels = new Stack<Point>();
            pixels.Push(pt);

            while (pixels.Count > 0)
            {
                Point a = pixels.Pop();
                if (a.X < 0 || a.X >= bmp.Width || a.Y < 0 || a.Y >= bmp.Height) continue;
                if (bmp.GetPixel(a.X, a.Y) != targetColor) continue;

                bmp.SetPixel(a.X, a.Y, replacementColor);

                pixels.Push(new Point(a.X - 1, a.Y));
                pixels.Push(new Point(a.X + 1, a.Y));
                pixels.Push(new Point(a.X, a.Y - 1));
                pixels.Push(new Point(a.X, a.Y + 1));
            }
            bmpTemp = (Bitmap)bmp.Clone();
            Invalidate();
        }


        // Прочие инструменты
        private void FormDocument_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = MainForm.GetCursorForTool(MainForm.CurrentTool);
            var scaledPoint = GetScaledPoint(e.X, e.Y);

            if (e.Button == MouseButtons.Left)
            {
                var pen = new Pen(MainForm.CurrentColor, MainForm.CurrentWidth);
                var eraser = new Pen(Color.White, MainForm.CurrentWidth);
                var brush = new SolidBrush(MainForm.CurrentColor);

                switch (MainForm.CurrentTool)
                {
                    case Tools.Eraser:
                        {
                            var g = Graphics.FromImage(bmp);
                            g.FillEllipse(new SolidBrush(Color.White),
                                new Rectangle(oldX, oldY, Math.Max(MainForm.CurrentWidth, 2), Math.Max(MainForm.CurrentWidth, 2)));
                            oldX = scaledPoint.X;
                            oldY = scaledPoint.Y;
                            IsChanged = true;
                            bmpTemp = bmp;

                            Invalidate();
                            break;
                        }
                    case Tools.Pen:
                        {
                            var g = Graphics.FromImage(bmp);
                            g.DrawLine(pen, oldX, oldY, scaledPoint.X, scaledPoint.Y);
                            oldX = scaledPoint.X;
                            oldY = scaledPoint.Y;
                            bmpTemp = bmp;
                            IsChanged = true;

                            Invalidate();
                            break;
                        }
                    case Tools.Line:
                        {
                            bmpTemp = (Bitmap)bmp.Clone();
                            var g = Graphics.FromImage(bmpTemp);
                            g.DrawLine(pen, oldX, oldY, scaledPoint.X, scaledPoint.Y);
                            IsChanged = true;

                            Invalidate();
                            break;
                        }
                    case Tools.Circle:
                        {
                            bmpTemp = (Bitmap)bmp.Clone();
                            var g = Graphics.FromImage(bmpTemp);
                            if (!MainForm.FillShapes)
                                g.DrawEllipse(pen, new Rectangle(oldX, oldY, scaledPoint.X - oldX, scaledPoint.Y - oldY));
                            else
                                g.FillEllipse(brush, new Rectangle(oldX, oldY, scaledPoint.X - oldX, scaledPoint.Y - oldY));
                            IsChanged = true;

                            Invalidate();
                            break;
                        }
                    case Tools.Rectangle:
                        {
                            bmpTemp = (Bitmap)bmp.Clone();
                            var g = Graphics.FromImage(bmpTemp);
                            if (!MainForm.FillShapes)
                                g.DrawRectangle(pen, new Rectangle(Math.Min(oldX, scaledPoint.X),
                                    Math.Min(oldY, scaledPoint.Y), Math.Abs(scaledPoint.X - oldX), Math.Abs(scaledPoint.Y - oldY)));
                            else
                                g.FillRectangle(brush, new Rectangle(Math.Min(oldX, scaledPoint.X),
                                    Math.Min(oldY, scaledPoint.Y), Math.Abs(scaledPoint.X - oldX), Math.Abs(scaledPoint.Y - oldY)));
                            IsChanged = true;

                            Invalidate();
                            break;
                        }
                    case Tools.Heart:
                        {
                            bmpTemp = (Bitmap)bmp.Clone();
                            var g = Graphics.FromImage(bmpTemp);
                            DrawHeart(g, oldX, oldY, scaledPoint.X, scaledPoint.Y, pen, brush, MainForm.FillShapes);
                            IsChanged = true;

                            Invalidate();
                            break;
                        }
                }
            }

            var parent = MdiParent as MainForm;
            parent?.ShowPosition(scaledPoint.X, scaledPoint.Y);
        }

        private void DrawHeart(Graphics g, int x1, int y1, int x2, int y2, Pen pen, Brush brush, bool isFill)
        {
            int x = Math.Min(x1, x2);
            int y = Math.Min(y1, y2);
            int w = Math.Abs(x2 - x1);
            int h = Math.Abs(y2 - y1);

            if (w <= 0 || h <= 0) 
                return;

            GraphicsPath p = new GraphicsPath();

            p.AddBezier(
                new Point(x + w / 2, y + h / 3),
                new Point(x + w, y),
                new Point(x + w, y + h / 2),
                new Point(x + w / 2, y + h)
            );

            p.AddBezier(
                new Point(x + w / 2, y + h),
                new Point(x, y + h / 2),
                new Point(x, y),
                new Point(x + w / 2, y + h / 3)
            );

            p.CloseFigure();

            if (isFill)
                g.FillPath(brush, p);
            else
                g.DrawPath(pen, p);
        }



        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (bmpTemp != null)
                DrawZoomedImage(e.Graphics, bmpTemp);
            else if (bmp != null)
                DrawZoomedImage(e.Graphics, bmp);
        }

        private void DrawZoomedImage(Graphics g, Bitmap image)
        {
            int newWidth = (int)(image.Width * zF);
            int newHeight = (int)(image.Height * zF);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
        }



        // Работа с файлами
        public void LoadImage(string path)
        {
            try
            {
                Bitmap loadedBmp = new Bitmap(path);
                bmp = new Bitmap(loadedBmp);
                bmpTemp = new Bitmap(bmp);
                CurrentFilePath = path;
                this.Text = Path.GetFileName(path);

                Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке изображения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void SaveAs(string path)
        {
            bmp.Save(path);
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                SaveAsDialog();
            }
            else
            {
                SaveAs(CurrentFilePath);
            }
        }
        public void SaveAsDialog()
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "JPG|*.jpg|BMP|*.bmp";
                dlg.DefaultExt = "jpg";
                dlg.AddExtension = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SaveAs(dlg.FileName);
                    IsChanged = false;
                }
            }
        }

        private void AutoSave(object sender, FormClosingEventArgs e)
        {
            if (IsChanged)
            {
                DialogResult result = MessageBox.Show("Сохранить изменения в файле?", "Внимание", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)                    
                    Save();               
                else if (result == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }
    }
}
