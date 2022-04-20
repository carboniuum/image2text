using IronOcr;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Image2Text
{
    public partial class Form : System.Windows.Forms.Form
    {
        private Point _rectStartPoint;
        private Rectangle _rect = new Rectangle();
        private readonly Brush _selectionBrush = new SolidBrush(Color.FromArgb(128, 72, 145, 220));

        private readonly IronTesseract _ocr = new IronTesseract(); 

        public Form()
        {
            InitializeComponent();

            _ocr.Language = OcrLanguage.EnglishBest;
            _ocr.Configuration.TesseractVersion = TesseractVersion.Tesseract5;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            _rectStartPoint = e.Location;
            Invalidate();
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            Point tempEndPoint = e.Location;

            _rect.Location = new Point(
                Math.Min(_rectStartPoint.X, tempEndPoint.X),
                Math.Min(_rectStartPoint.Y, tempEndPoint.Y)
            );
            _rect.Size = new Size(
                Math.Abs(_rectStartPoint.X - tempEndPoint.X),
                Math.Abs(_rectStartPoint.Y - tempEndPoint.Y)
            );

            pictureBox.Invalidate();
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_rect.Contains(e.Location))
                {
                    var croppedImage = GetCroppedImage(new Bitmap(pictureBox.Image), _rect);

                    using (var ocrInput = new OcrInput())
                    {
                        ocrInput.AddImage(croppedImage);
                        var result = _ocr.Read(ocrInput);
                        string txt = result.Text;
                    }
                }
            }
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox.Image != null)
            {
                if (_rect != null && _rect.Width > 0 && _rect.Height > 0)
                {
                    e.Graphics.FillRectangle(_selectionBrush, _rect);
                }
            }
        }

        private Bitmap GetCroppedImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }

        private void panel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    pictureBox.Left -= 10;
                    break;
                case Keys.Right:
                    pictureBox.Left += 10;
                    break;
                case Keys.Up:
                    pictureBox.Top += 10;
                    break;
                case Keys.Down:
                    pictureBox.Top -= 10;
                    break;
            }
        }
    }
}
