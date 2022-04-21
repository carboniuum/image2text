using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using IronOcr;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Image2Text.Helpers;

namespace Image2Text
{
    public partial class Form : System.Windows.Forms.Form
    {
        private Point _rectStartPoint;
        private Rectangle _rect = new Rectangle();
        private readonly Brush _selectionBrush = new SolidBrush(Color.FromArgb(128, 218, 91, 160));

        private readonly IronTesseract _ocr = new IronTesseract();

        private readonly PictureBox _pb = new PictureBox();
        private readonly NoFocusTrackBar _trackBar = new NoFocusTrackBar();
        private const double TrackBarStep = 10.0;

        public Form()
        {
            InitializeComponent();

            _trackBar.Location = new Point(12, 12);
            _trackBar.Size = new Size(300, 45);
            _trackBar.Minimum = 0;
            _trackBar.Maximum = 20;
            _trackBar.SmallChange = 1;
            _trackBar.LargeChange = 1;
            _trackBar.TickFrequency = 1;
            _trackBar.Value = (int)TrackBarStep;
            _trackBar.Scroll += (s, e) =>
            {
                toolTip.SetToolTip(_trackBar, $"{_trackBar.Value * 10}%");
                if (_trackBar.Value > 0)
                {
                    pictureBox.Image = null;

                    if (_pb.Image != null)
                    {
                        pictureBox.Image = GetZoomedImage(_pb.Image, _trackBar.Value / TrackBarStep, _trackBar.Value / TrackBarStep);
                    }
                }
            };
            Controls.Add(_trackBar);

            comboBox.Items.AddRange(Langs.Dict.Select(x => x.Key).ToArray());
            comboBox.SelectedItem = comboBox.Items[0];

            _ocr.Configuration.TesseractVersion = TesseractVersion.Tesseract5;
            _ocr.Language = Langs.Dict[comboBox.SelectedItem.ToString()];
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _ocr.Language = Langs.Dict[comboBox.SelectedItem.ToString()];
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
                        textBox.Text = result.Text;
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

        private Image GetZoomedImage(Image image, double x, double y)
        {
            var bitmap = new Bitmap(image, (int)(image.Width * x), (int)(image.Height * y));

            using (var g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                return bitmap;
            }
        }

        // Drag & Drop Image
        private void panel_DragDrop(object sender, DragEventArgs e)
        {
            dragAndDropLabel.Text = String.Empty;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var allowedExtensions = new List<string> { ".png", ".jpg", ".jpeg" };

            if (!String.IsNullOrEmpty(files[0]))
            {
                string ext = Path.GetExtension(files[0]);
                if (!allowedExtensions.Contains(ext.ToLower()))
                {
                    dragAndDropLabel.Text = "File has invalid format";
                }
                else
                {
                    dragAndDropLabel.Visible = false;
                    pictureBox.Visible = true;
                    pictureBox.Image = new Bitmap(files[0]);
                    _pb.Image = pictureBox.Image;
                }
            } 
        }

        private void panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                dragAndDropLabel.Text = "Release a mouse";
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void panel_DragLeave(object sender, EventArgs e)
        {
            dragAndDropLabel.Text = "Just drop images here";
        }

        private void panel_Paint(object sender, PaintEventArgs e)
        {
            var pen = new Pen(Color.WhiteSmoke, 3);
            pen.DashPattern = new float[] { 4f, 2f };
            e.Graphics.DrawRectangle(pen, 1, 1, panel.Width - 3, panel.Height - 3);
        }
    }

    // Custom trackbar without default annoying dashed outline ))
    class NoFocusTrackBar : TrackBar
    {
        [DllImport("user32.dll")]
        public extern static int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        private static int MakeParam(int loWord, int hiWord)
        {
            return (hiWord << 16) | (loWord & 0xffff);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            SendMessage(Handle, 0x0128, MakeParam(1, 0x1), 0);
        }
    }
}
