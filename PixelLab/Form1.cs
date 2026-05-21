using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using System.Linq;

namespace PixelLab
{
    public partial class Form1 : Form
    {
        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        Image<Bgr, byte> originalImage;
        Mat convertedImage = new Mat();
        VectorOfMat channels = new VectorOfMat();
        string imageFilePath = "";

        ComboBox comboColorsCount;
        Timer updateTimer;

        PictureBox pictureOriginal;
        PictureBox pictureProcessed;
        PictureBox pictureC1;
        PictureBox pictureC2;
        PictureBox pictureC3;

        Button btnLoad;
        Button btnSpace;
        Button btnRotate;
        Button btnInfo;
        Button btnReset;
        Button btnSave;
        Button btnReal3D;
        ComboBox comboColorSpace;

        CheckBox chkC1;
        CheckBox chkC2;
        CheckBox chkC3;

        TrackBar trackC1;
        TrackBar trackC2;
        TrackBar trackC3;

        Label lbl1;
        Label lbl2;
        Label lbl3;
        Label lblRGB;
        Label lblHSV;
        Label lblLAB;
        Label lblYCrCb;
        Label lblCMYK;
        Label lblYUV;
        Label lblColorInfo;

        float zoomFactor = 1.0f;
        float zoomFactorOriginal = 1.0f;

        private Color QuantizeColor(Color c, int levels)
        {
            int step = 256 / levels;
            int r = (c.R / step) * step;
            int g = (c.G / step) * step;
            int b = (c.B / step) * step;

            r = Clamp(r, 0, 255);
            g = Clamp(g, 0, 255);
            b = Clamp(b, 0, 255);

            return Color.FromArgb(r, g, b);
        }

        public Form1()
        {
            InitializeComponent();
            InitializeUI();

            comboColorsCount = new ComboBox();
            comboColorsCount.Location = new Point(950, 160);
            comboColorsCount.Items.Add("Original (256)");
            comboColorsCount.Items.Add("2 Colors");
            comboColorsCount.Items.Add("4 Colors");
            comboColorsCount.Items.Add("8 Colors");
            comboColorsCount.Items.Add("16 Colors");
            comboColorsCount.SelectedIndex = 2;
            comboColorsCount.SelectedIndexChanged += ChannelControlChanged;
            this.Controls.Add(comboColorsCount);

            updateTimer = new Timer();
            updateTimer.Interval = 50;
            updateTimer.Tick += (s, e) =>
            {
                updateTimer.Stop();
                ProcessImage();
            };

            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;
        }

        private void InitializeUI()
        {
            this.Text = "PixelLab";
            this.Width = 1400;
            this.Height = 1000;

            pictureOriginal = new PictureBox();
            pictureOriginal.Location = new Point(20, 20);
            pictureOriginal.Size = new Size(400, 300);
            pictureOriginal.BorderStyle = BorderStyle.FixedSingle;
            pictureOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            pictureOriginal.MouseClick += PictureOriginal_MouseClick;
            pictureOriginal.MouseWheel += PictureOriginal_MouseWheel;
            this.Controls.Add(pictureOriginal);

            pictureProcessed = new PictureBox();
            pictureProcessed.Location = new Point(450, 20);
            pictureProcessed.Size = new Size(400, 300);
            pictureProcessed.BorderStyle = BorderStyle.FixedSingle;
            pictureProcessed.SizeMode = PictureBoxSizeMode.Zoom;
            pictureProcessed.MouseClick += PictureProcessed_MouseClick;
            pictureProcessed.MouseWheel += PictureProcessed_MouseWheel;
            this.Controls.Add(pictureProcessed);
            pictureProcessed.Focus();

            pictureC1 = CreateChannelBox(20, 380);
            pictureC2 = CreateChannelBox(320, 380);
            pictureC3 = CreateChannelBox(620, 380);

            btnLoad = new Button();
            btnLoad.Text = "Load Image";
            btnLoad.Location = new Point(950, 50);
            btnLoad.Size = new Size(200, 40);
            btnLoad.Click += BtnLoad_Click;
            this.Controls.Add(btnLoad);

            comboColorSpace = new ComboBox();
            comboColorSpace.Location = new Point(950, 120);
            comboColorSpace.Width = 200;
            comboColorSpace.Items.Add("RGB");
            comboColorSpace.Items.Add("HSV");
            comboColorSpace.Items.Add("LAB");
            comboColorSpace.Items.Add("YCbCr");
            comboColorSpace.Items.Add("CMYK");
            comboColorSpace.Items.Add("YUV");
            comboColorSpace.SelectedIndex = 0;
            comboColorSpace.SelectedIndexChanged += ComboColorSpace_SelectedIndexChanged;
            this.Controls.Add(comboColorSpace);

            chkC1 = CreateCheckBox("Channel 1", 950, 220);
            chkC2 = CreateCheckBox("Channel 2", 950, 270);
            chkC3 = CreateCheckBox("Channel 3", 950, 320);
            chkC1.CheckedChanged += ChannelControlChanged;
            chkC2.CheckedChanged += ChannelControlChanged;
            chkC3.CheckedChanged += ChannelControlChanged;

            trackC1 = CreateTrackBar(1070, 210);
            trackC2 = CreateTrackBar(1070, 260);
            trackC3 = CreateTrackBar(1070, 310);
            trackC1.Scroll += ChannelControlChanged;
            trackC2.Scroll += ChannelControlChanged;
            trackC3.Scroll += ChannelControlChanged;

            btnSpace = new Button();
            btnSpace.Text = "3D Chart Spaces";
            btnSpace.Location = new Point(950, 370);
            btnSpace.Size = new Size(200, 40);
            btnSpace.Click += BtnSpace_Click;
            this.Controls.Add(btnSpace);

            btnReal3D = new Button();
            btnReal3D.Text = "Real 3D Spaces";
            btnReal3D.Location = new Point(950, 415);
            btnReal3D.Size = new Size(200, 40);
            btnReal3D.Click += BtnReal3D_Click;
            this.Controls.Add(btnReal3D);

            btnRotate = new Button();
            btnRotate.Text = "Rotate 90";
            btnRotate.Location = new Point(950, 460);
            btnRotate.Size = new Size(200, 40);
            btnRotate.Click += BtnRotate_Click;
            this.Controls.Add(btnRotate);

            btnInfo = new Button();
            btnInfo.Text = "Image Info";
            btnInfo.Location = new Point(950, 505);
            btnInfo.Size = new Size(200, 40);
            btnInfo.Click += BtnInfo_Click;
            this.Controls.Add(btnInfo);

            btnReset = new Button();
            btnReset.Text = "Reset";
            btnReset.Location = new Point(950, 550);
            btnReset.Size = new Size(200, 40);
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            btnSave = new Button();
            btnSave.Text = "Save Image";
            btnSave.Location = new Point(950, 595);
            btnSave.Size = new Size(200, 40);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            lblRGB = CreateLabel("", 950, 640);
            lblHSV = CreateLabel("", 950, 665);
            lblLAB = CreateLabel("", 950, 690);
            lblYCrCb = CreateLabel("", 950, 715);
            lblCMYK = CreateLabel("", 950, 740);
            lblYUV = CreateLabel("", 950, 765);

            lbl1 = CreateLabel("C1", 20, 340);
            lbl2 = CreateLabel("C2", 320, 340);
            lbl3 = CreateLabel("C3", 620, 340);

            lblColorInfo = new Label();
            lblColorInfo.Location = new Point(20, 870);
            lblColorInfo.Size = new Size(1200, 60);
            lblColorInfo.Font = new Font("Arial", 10, FontStyle.Bold);
            lblColorInfo.BorderStyle = BorderStyle.FixedSingle;
            lblColorInfo.Text = "Click on original image to show all color values";
            this.Controls.Add(lblColorInfo);
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            trackC1.Value = 0;
            trackC2.Value = 0;
            trackC3.Value = 0;

            chkC1.Checked = true;
            chkC2.Checked = true;
            chkC3.Checked = true;

            comboColorSpace.SelectedIndex = 0;
            comboColorsCount.SelectedIndex = 0;

            zoomFactor = 1.0f;
            zoomFactorOriginal = 1.0f;
            pictureOriginal.Width = 400;
            pictureOriginal.Height = 300;
            pictureProcessed.Width = 400;
            pictureProcessed.Height = 300;

            ProcessImage();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (pictureProcessed.Image == null)
            {
                MessageBox.Show("No processed image to save.", "Save Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";
            sfd.Title = "Save Processed Image";
            sfd.FileName = "processed_image";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(sfd.FileName).ToLower();
                ImageFormat format = ImageFormat.Png;

                if (ext == ".jpg" || ext == ".jpeg")
                    format = ImageFormat.Jpeg;
                else if (ext == ".bmp")
                    format = ImageFormat.Bmp;
                else
                    format = ImageFormat.Png;

                pictureProcessed.Image.Save(sfd.FileName, format);
                MessageBox.Show("Image saved successfully!", "Save Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnInfo_Click(object sender, EventArgs e)
        {
            if (originalImage == null || string.IsNullOrEmpty(imageFilePath) || !File.Exists(imageFilePath))
            {
                MessageBox.Show("No image loaded or file not found.", "Image Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FileInfo fi = new FileInfo(imageFilePath);
            string name = fi.Name;
            string extension = fi.Extension.ToUpper().Replace(".", "");
            long sizeBytes = fi.Length;
            double sizeKB = sizeBytes / 1024.0;
            double sizeMB = sizeKB / 1024.0;
            string sizeStr = sizeMB >= 1.0 ? $"{sizeMB:F2} MB" : $"{sizeKB:F2} KB";

            int width = originalImage.Width;
            int height = originalImage.Height;
            string dimensions = $"{width} x {height}";
            string totalPixels = $"{width * height:N0} pixels";

            string message =
                $"File Name: {name}\n" +
                $"Format: {extension}\n" +
                $"Storage Size: {sizeStr}\n" +
                $"Dimensions: {dimensions}\n" +
                $"Total Pixels: {totalPixels}\n\n" +
                $"Full Path:\n{fi.FullName}";

            MessageBox.Show(message, "Image Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private PictureBox CreateChannelBox(int x, int y)
        {
            PictureBox pb = new PictureBox();
            pb.Location = new Point(x, y);
            pb.Size = new Size(250, 250);
            pb.BorderStyle = BorderStyle.FixedSingle;
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            this.Controls.Add(pb);
            return pb;
        }

        private CheckBox CreateCheckBox(string text, int x, int y)
        {
            CheckBox chk = new CheckBox();
            chk.Text = text;
            chk.Location = new Point(x, y);
            chk.Checked = true;
            this.Controls.Add(chk);
            return chk;
        }

        private TrackBar CreateTrackBar(int x, int y)
        {
            TrackBar tb = new TrackBar();
            tb.Location = new Point(x, y);
            tb.Width = 200;
            tb.Minimum = -100;
            tb.Maximum = 100;
            tb.Value = 0;
            this.Controls.Add(tb);
            return tb;
        }

        private Label CreateLabel(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            lbl.Font = new Font("Arial", 9, FontStyle.Bold);
            this.Controls.Add(lbl);
            return lbl;
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images|*.jpg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadImage(ofd.FileName);
            }
        }

        private void LoadImage(string path)
        {
            imageFilePath = path;
            originalImage = new Image<Bgr, byte>(path);
            pictureOriginal.Image = originalImage.ToBitmap();
            zoomFactorOriginal = 1.0f;
            pictureOriginal.Width = 400;
            pictureOriginal.Height = 300;
            ProcessImage();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            LoadImage(files[0]);
        }

        private void ComboColorSpace_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProcessImage();
        }

        private void ChannelControlChanged(object sender, EventArgs e)
        {
            updateTimer.Stop();
            updateTimer.Start();
        }

        private (int C, int M, int Y, int K) RgbToCmyk(int r, int g, int b)
        {
            float R = r / 255f;
            float G = g / 255f;
            float B = b / 255f;
            float K = 1 - Math.Max(Math.Max(R, G), B);
            float C, M, Y;
            if (K < 1f)
            {
                C = (1f - R - K) / (1f - K);
                M = (1f - G - K) / (1f - K);
                Y = (1f - B - K) / (1f - K);
            }
            else
            {
                C = 0f; M = 0f; Y = 0f;
            }
            return ((int)(C * 100), (int)(M * 100), (int)(Y * 100), (int)(K * 100));
        }

        private void ProcessImage()
        {
            if (originalImage == null)
                return;

            channels.Clear();
            string mode = comboColorSpace.SelectedItem.ToString();

            if (mode == "CMYK")
            {
                Image<Gray, byte> cImg = new Image<Gray, byte>(originalImage.Size);
                Image<Gray, byte> mImg = new Image<Gray, byte>(originalImage.Size);
                Image<Gray, byte> yImg = new Image<Gray, byte>(originalImage.Size);
                Image<Gray, byte> kImg = new Image<Gray, byte>(originalImage.Size);

                for (int i = 0; i < originalImage.Rows; i++)
                {
                    for (int j = 0; j < originalImage.Cols; j++)
                    {
                        Bgr pixel = originalImage[i, j];
                        float R = (float)(pixel.Red / 255f);
                        float G = (float)(pixel.Green / 255f);
                        float B = (float)(pixel.Blue / 255f);

                        float K = 1 - Math.Max(Math.Max(R, G), B);
                        float C, M, Y;
                        if (K < 1f)
                        {
                            C = (1f - R - K) / (1f - K);
                            M = (1f - G - K) / (1f - K);
                            Y = (1f - B - K) / (1f - K);
                        }
                        else
                        {
                            C = 0f; M = 0f; Y = 0f;
                        }

                        cImg.Data[i, j, 0] = (byte)Clamp((int)(C * 255), 0, 255);
                        mImg.Data[i, j, 0] = (byte)Clamp((int)(M * 255), 0, 255);
                        yImg.Data[i, j, 0] = (byte)Clamp((int)(Y * 255), 0, 255);
                        kImg.Data[i, j, 0] = (byte)Clamp((int)(K * 255), 0, 255);
                    }
                }

                channels.Push(cImg.Mat);
                channels.Push(mImg.Mat);
                channels.Push(yImg.Mat);
                channels.Push(kImg.Mat);

                lbl1.Text = "Cyan";
                lbl2.Text = "Magenta";
                lbl3.Text = "Yellow";
            }
            else
            {
                if (mode == "RGB")
                {
                    convertedImage = originalImage.Mat.Clone();
                    CvInvoke.Split(convertedImage, channels);
                    lbl1.Text = "Blue";
                    lbl2.Text = "Green";
                    lbl3.Text = "Red";
                }
                else if (mode == "HSV")
                {
                    CvInvoke.CvtColor(originalImage, convertedImage, ColorConversion.Bgr2Hsv);
                    CvInvoke.Split(convertedImage, channels);
                    lbl1.Text = "Hue";
                    lbl2.Text = "Saturation";
                    lbl3.Text = "Value";
                }
                else if (mode == "LAB")
                {
                    CvInvoke.CvtColor(originalImage, convertedImage, ColorConversion.Bgr2Lab);
                    CvInvoke.Split(convertedImage, channels);
                    lbl1.Text = "L";
                    lbl2.Text = "A";
                    lbl3.Text = "B";
                }
                else if (mode == "YCbCr")
                {
                    CvInvoke.CvtColor(originalImage, convertedImage, ColorConversion.Bgr2YCrCb);
                    CvInvoke.Split(convertedImage, channels);
                    lbl1.Text = "Y";
                    lbl2.Text = "Cr";
                    lbl3.Text = "Cb";
                }
                else if (mode == "YUV")
                {
                    CvInvoke.CvtColor(originalImage, convertedImage, ColorConversion.Bgr2Yuv);
                    CvInvoke.Split(convertedImage, channels);
                    lbl1.Text = "Y";
                    lbl2.Text = "U";
                    lbl3.Text = "V";
                }
            }

            pictureC1.Image = channels[0].ToBitmap();
            pictureC2.Image = channels[1].ToBitmap();
            pictureC3.Image = channels[2].ToBitmap();

            if (!chkC1.Checked) channels[0].SetTo(new MCvScalar(0));
            if (!chkC2.Checked) channels[1].SetTo(new MCvScalar(0));
            if (!chkC3.Checked) channels[2].SetTo(new MCvScalar(0));

            AddValueToChannel(channels[0], trackC1.Value);
            AddValueToChannel(channels[1], trackC2.Value);
            AddValueToChannel(channels[2], trackC3.Value);

            Mat merged = new Mat();
            CvInvoke.Merge(channels, merged);

            if (mode == "CMYK")
            {
                Image<Gray, byte> cImg = channels[0].ToImage<Gray, byte>();
                Image<Gray, byte> mImg = channels[1].ToImage<Gray, byte>();
                Image<Gray, byte> yImg = channels[2].ToImage<Gray, byte>();
                Image<Gray, byte> kImg = channels[3].ToImage<Gray, byte>();

                Image<Bgr, byte> bgrResult = new Image<Bgr, byte>(originalImage.Size);
                for (int i = 0; i < originalImage.Rows; i++)
                {
                    for (int j = 0; j < originalImage.Cols; j++)
                    {
                        float c = cImg.Data[i, j, 0] / 255f;
                        float m = mImg.Data[i, j, 0] / 255f;
                        float y = yImg.Data[i, j, 0] / 255f;
                        float k = kImg.Data[i, j, 0] / 255f;

                        int r = (int)((1f - c) * (1f - k) * 255f);
                        int g = (int)((1f - m) * (1f - k) * 255f);
                        int b = (int)((1f - y) * (1f - k) * 255f);

                        r = Clamp(r, 0, 255);
                        g = Clamp(g, 0, 255);
                        b = Clamp(b, 0, 255);

                        bgrResult[i, j] = new Bgr((byte)b, (byte)g, (byte)r);
                    }
                }

                merged = bgrResult.Mat;
            }

            Bitmap bmp = merged.ToImage<Bgr, byte>().ToBitmap();

            int levels = 256;
            if (comboColorsCount.SelectedItem != null)
            {
                string selected = comboColorsCount.SelectedItem.ToString();
                if (selected.Contains("Original")) levels = 256;
                else if (selected.Contains("2")) levels = 2;
                else if (selected.Contains("4")) levels = 4;
                else if (selected.Contains("8")) levels = 8;
                else if (selected.Contains("16")) levels = 16;
            }

            if (levels < 256)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        Color q = QuantizeColor(c, levels);
                        bmp.SetPixel(x, y, q);
                    }
                }
            }

            pictureProcessed.Image = bmp;
        }

        private void AddValueToChannel(Mat channel, int value)
        {
            Image<Gray, byte> img = channel.ToImage<Gray, byte>();
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    int newValue = img.Data[y, x, 0] + value;
                    if (newValue > 255) newValue = 255;
                    if (newValue < 0) newValue = 0;
                    img.Data[y, x, 0] = (byte)newValue;
                }
            }
            img.Mat.CopyTo(channel);
        }

        private void UpdateColorInfoLabels(int r, int g, int b)
        {
            Image<Bgr, byte> pixel = new Image<Bgr, byte>(1, 1, new Bgr((byte)b, (byte)g, (byte)r));

            Mat hsvMat = new Mat();
            CvInvoke.CvtColor(pixel, hsvMat, ColorConversion.Bgr2Hsv);
            Hsv hsv = hsvMat.ToImage<Hsv, byte>()[0, 0];
            int h = (int)(hsv.Hue * 2.0);
            int s = (int)(hsv.Satuation * 100.0 / 255.0);
            int v = (int)(hsv.Value * 100.0 / 255.0);
            s = Clamp(s, 0, 100);
            v = Clamp(v, 0, 100);

            Mat labMat = new Mat();
            CvInvoke.CvtColor(pixel, labMat, ColorConversion.Bgr2Lab);
            Lab lab = labMat.ToImage<Lab, byte>()[0, 0];

            Mat yccMat = new Mat();
            CvInvoke.CvtColor(pixel, yccMat, ColorConversion.Bgr2YCrCb);
            Ycc ycc = yccMat.ToImage<Ycc, byte>()[0, 0];

            var cmyk = RgbToCmyk(r, g, b);

            Mat yuvMat = new Mat();
            CvInvoke.CvtColor(pixel, yuvMat, ColorConversion.Bgr2Yuv);
            Image<Gray, byte>[] yuvCh = yuvMat.ToImage<Bgr, byte>().Split();
            int Y = yuvCh[0].Data[0, 0, 0];
            int U = yuvCh[1].Data[0, 0, 0];
            int V = yuvCh[2].Data[0, 0, 0];
            yuvCh[0].Dispose(); yuvCh[1].Dispose(); yuvCh[2].Dispose();

            pixel.Dispose();

            lblRGB.Text = $"RGB → ({r}, {g}, {b})";
            lblHSV.Text = $"HSV → ({h}°, {s}%, {v}%)";
            lblLAB.Text = $"LAB → (L:{(int)lab.X}, a:{(int)lab.Y}, b:{(int)lab.Z})";
            lblYCrCb.Text = $"YCbCr → (Y:{(int)ycc.Y}, Cr:{(int)ycc.Cr}, Cb:{(int)ycc.Cb})";
            lblCMYK.Text = $"CMYK → (C:{cmyk.C}%, M:{cmyk.M}%, Y:{cmyk.Y}%, K:{cmyk.K}%)";
            lblYUV.Text = $"YUV → (Y:{Y}, U:{U}, V:{V})";

            lblColorInfo.Text =
                $"RGB → ({r}, {g}, {b})  " +
                $"HSV → ({h}°, {s}%, {v}%)  " +
                $"LAB → (L:{(int)lab.X}, a:{(int)lab.Y}, b:{(int)lab.Z})  " +
                $"YCbCr → (Y:{(int)ycc.Y}, Cr:{(int)ycc.Cr}, Cb:{(int)ycc.Cb})  " +
                $"CMYK → (C:{cmyk.C}%, M:{cmyk.M}%, Y:{cmyk.Y}%, K:{cmyk.K}%)  " +
                $"YUV → (Y:{Y}, U:{U}, V:{V})";
        }

        private void PictureOriginal_MouseClick(object sender, MouseEventArgs e)
        {
            if (originalImage == null) return;

            int imgX = e.X * originalImage.Width / pictureOriginal.Width;
            int imgY = e.Y * originalImage.Height / pictureOriginal.Height;
            if (imgX < 0 || imgX >= originalImage.Width || imgY < 0 || imgY >= originalImage.Height) return;

            Bgr rgb = originalImage[imgY, imgX];
            int r = (int)rgb.Red;
            int g = (int)rgb.Green;
            int b = (int)rgb.Blue;

            UpdateColorInfoLabels(r, g, b);
        }

        private void PictureProcessed_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureProcessed.Image == null) return;

            Bitmap bmp = new Bitmap(pictureProcessed.Image);
            int x = e.X * bmp.Width / pictureProcessed.Width;
            int y = e.Y * bmp.Height / pictureProcessed.Height;
            if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height) return;

            Color c = bmp.GetPixel(x, y);
            int r = c.R, g = c.G, b = c.B;

            UpdateColorInfoLabels(r, g, b);
        }

        private void BtnRotate_Click(object sender, EventArgs e)
        {
            if (pictureProcessed.Image == null) return;
            pictureProcessed.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureProcessed.Refresh();
        }

        private void PictureOriginal_MouseWheel(object sender, MouseEventArgs e)
        {
            if (originalImage == null) return;

            if (e.Delta > 0) zoomFactorOriginal += 0.1f;
            else zoomFactorOriginal -= 0.1f;
            if (zoomFactorOriginal < 0.1f) zoomFactorOriginal = 0.1f;

            pictureOriginal.Width = (int)(400 * zoomFactorOriginal);
            pictureOriginal.Height = (int)(300 * zoomFactorOriginal);
        }

        private void PictureProcessed_MouseWheel(object sender, MouseEventArgs e)
        {
            if (pictureProcessed.Image == null) return;

            if (e.Delta > 0) zoomFactor += 0.1f;
            else zoomFactor -= 0.1f;
            if (zoomFactor < 0.1f) zoomFactor = 0.1f;

            pictureProcessed.Width = (int)(400 * zoomFactor);
            pictureProcessed.Height = (int)(300 * zoomFactor);
        }

        private Chart Create3DChart(string title, string xTitle, string yTitle, out ChartArea area)
        {
            Chart chart = new Chart();
            chart.Dock = DockStyle.Fill;

            area = new ChartArea();
            area.Area3DStyle.Enable3D = true;
            area.Area3DStyle.IsClustered = true;
            area.AxisX.Title = xTitle;
            area.AxisY.Title = yTitle;

            chart.ChartAreas.Add(area);

            Series series = new Series();
            series.ChartType = SeriesChartType.Bubble;
            series.MarkerStyle = MarkerStyle.Circle;
            chart.Series.Add(series);

            return chart;
        }

        private void MakeChartInteractive(Chart chart, ChartArea area, Action<Bgr> onColorSelected)
        {
            bool isMouseDown = false;
            bool hasDragged = false;
            Point startMousePos = Point.Empty;
            int startRotation = 0;
            int startInclination = 0;
            double zoomFactorLocal = 1.0;
            double originalXMin = 0, originalXMax = 0;
            double originalYMin = 0, originalYMax = 0;
            bool axisRangeSet = false;

            const int DRAG_THRESHOLD = 5;

            void EnsureAxisRange()
            {
                if (!axisRangeSet && chart.Series[0].Points.Count > 0)
                {
                    var pts = chart.Series[0].Points;
                    double xMin = double.MaxValue, xMax = double.MinValue;
                    double yMin = double.MaxValue, yMax = double.MinValue;
                    foreach (var p in pts)
                    {
                        if (p.XValue < xMin) xMin = p.XValue;
                        if (p.XValue > xMax) xMax = p.XValue;
                        if (p.YValues[0] < yMin) yMin = p.YValues[0];
                        if (p.YValues[0] > yMax) yMax = p.YValues[0];
                    }
                    originalXMin = xMin - 5;
                    originalXMax = xMax + 5;
                    originalYMin = yMin - 5;
                    originalYMax = yMax + 5;
                    area.AxisX.Minimum = originalXMin;
                    area.AxisX.Maximum = originalXMax;
                    area.AxisY.Minimum = originalYMin;
                    area.AxisY.Maximum = originalYMax;
                    axisRangeSet = true;
                }
            }

            chart.PrePaint += (s, e) => EnsureAxisRange();

            chart.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isMouseDown = true;
                    hasDragged = false;
                    startMousePos = e.Location;
                    startRotation = area.Area3DStyle.Rotation;
                    startInclination = area.Area3DStyle.Inclination;
                }
            };

            chart.MouseMove += (s, e) =>
            {
                if (isMouseDown)
                {
                    int dx = e.X - startMousePos.X;
                    int dy = e.Y - startMousePos.Y;
                    if (!hasDragged && (Math.Abs(dx) >= DRAG_THRESHOLD || Math.Abs(dy) >= DRAG_THRESHOLD))
                    {
                        hasDragged = true;
                    }
                    if (hasDragged)
                    {
                        int newRotation = startRotation - dx / 2;
                        int newInclination = startInclination + dy / 2;
                        if (newRotation < -180) newRotation += 360;
                        if (newRotation > 180) newRotation -= 360;
                        if (newInclination < -90) newInclination = -90;
                        if (newInclination > 90) newInclination = 90;
                        area.Area3DStyle.Rotation = newRotation;
                        area.Area3DStyle.Inclination = newInclination;
                    }
                }
            };

            chart.MouseUp += (s, e) =>
            {
                if (!isMouseDown) return;
                isMouseDown = false;

                if (!hasDragged)
                {
                    HitTestResult hit = chart.HitTest(e.X, e.Y);
                    if (hit.ChartElementType == ChartElementType.DataPoint)
                    {
                        var point = chart.Series[0].Points[hit.PointIndex];
                        if (point.Tag is Bgr color)
                            onColorSelected?.Invoke(color);
                    }
                }
            };

            chart.MouseWheel += (s, e) =>
            {
                EnsureAxisRange();
                if (!axisRangeSet) return;

                double scale = (e.Delta > 0) ? 0.97 : 1.03;
                zoomFactorLocal *= scale;

                double xHalf = (originalXMax - originalXMin) * zoomFactorLocal / 2;
                double xCenter = (originalXMax + originalXMin) / 2;
                double yHalf = (originalYMax - originalYMin) * zoomFactorLocal / 2;
                double yCenter = (originalYMax + originalYMin) / 2;

                area.AxisX.Minimum = xCenter - xHalf;
                area.AxisX.Maximum = xCenter + xHalf;
                area.AxisY.Minimum = yCenter - yHalf;
                area.AxisY.Maximum = yCenter + yHalf;
            };
        }

        private Label CreateColorInfoLabel()
        {
            Label lbl = new Label();
            lbl.Dock = DockStyle.Fill;
            lbl.Font = new Font("Consolas", 11, FontStyle.Bold);
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            lbl.Text = "Click a point to see all color values";
            return lbl;
        }

        private void ShowRGBSpace(Image<Bgr, byte> img)
        {
            Form f = new Form();
            f.Text = "3D RGB Space";
            f.Width = 900;
            f.Height = 750;

            ChartArea area;
            Chart chart = Create3DChart("RGB", "RED", "GREEN", out area);
            Series series = chart.Series[0];

            for (int y = 0; y < img.Height; y += 3)
            {
                for (int x = 0; x < img.Width; x += 3)
                {
                    Bgr p = img[y, x];
                    int r = (int)p.Red;
                    int g = (int)p.Green;
                    int b = (int)p.Blue;
                    int index = series.Points.AddXY(r, g);
                    series.Points[index].YValues = new double[] { g, b };
                    series.Points[index].Color = Color.FromArgb(r, g, b);
                    series.Points[index].MarkerSize = Math.Max(3, b / 15);
                    series.Points[index].Tag = p;
                }
            }

            chart.Legends.Clear();

            Label infoLabel = CreateColorInfoLabel();
            MakeChartInteractive(chart, area, (Bgr color) =>
            {
                int r = (int)color.Red;
                int g = (int)color.Green;
                int b = (int)color.Blue;
                Image<Bgr, byte> pixelImg = new Image<Bgr, byte>(1, 1);
                pixelImg[0, 0] = color;
                Image<Hsv, byte> hsvImg = pixelImg.Convert<Hsv, byte>();
                Hsv hsv = hsvImg[0, 0];
                int h = (int)(hsv.Hue * 2.0);
                int s = (int)(hsv.Satuation * 100.0 / 255.0);
                int v = (int)(hsv.Value * 100.0 / 255.0);
                s = Clamp(s, 0, 100);
                v = Clamp(v, 0, 100);
                Image<Lab, byte> labImg = pixelImg.Convert<Lab, byte>();
                Lab lab = labImg[0, 0];
                var cmyk = RgbToCmyk(r, g, b);
                Image<Bgr, byte> yuvImg = pixelImg.Convert<Bgr, byte>();
                Mat yuvMat = new Mat();
                CvInvoke.CvtColor(yuvImg, yuvMat, ColorConversion.Bgr2Yuv);
                Image<Gray, byte>[] yuvCh = yuvMat.ToImage<Bgr, byte>().Split();
                int Y = yuvCh[0].Data[0, 0, 0];
                int U = yuvCh[1].Data[0, 0, 0];
                int V = yuvCh[2].Data[0, 0, 0];
                yuvCh[0].Dispose(); yuvCh[1].Dispose(); yuvCh[2].Dispose();
                Image<Ycc, byte> yccImg = pixelImg.Convert<Ycc, byte>();
                Ycc ycc = yccImg[0, 0];
                pixelImg.Dispose();
                infoLabel.Text =
                    $"RGB → ({r}, {g}, {b})" + Environment.NewLine +
                    $"HSV → ({h}°, {s}%, {v}%)" + Environment.NewLine +
                    $"LAB → (L:{(int)lab.X}, a:{(int)lab.Y}, b:{(int)lab.Z})" + Environment.NewLine +
                    $"YCbCr → (Y:{(int)ycc.Y}, Cr:{(int)ycc.Cr}, Cb:{(int)ycc.Cb})" + Environment.NewLine +
                    $"CMYK → (C:{cmyk.C}%, M:{cmyk.M}%, Y:{cmyk.Y}%, K:{cmyk.K}%)" + Environment.NewLine +
                    $"YUV → (Y:{Y}, U:{U}, V:{V})";
            });

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 120;
            panel.Controls.Add(infoLabel);
            f.Controls.Add(chart);
            f.Controls.Add(panel);
            panel.BringToFront();

            f.ShowDialog();
        }

        private void ShowHSVSpace(Image<Bgr, byte> img)
        {
            Image<Hsv, byte> hsvImg = img.Convert<Hsv, byte>();
            Form f = new Form();
            f.Text = "3D HSV Space";
            f.Width = 900;
            f.Height = 750;

            ChartArea area;
            Chart chart = Create3DChart("HSV", "HUE", "SATURATION", out area);
            Series series = chart.Series[0];

            for (int y = 0; y < hsvImg.Height; y += 3)
            {
                for (int x = 0; x < hsvImg.Width; x += 3)
                {
                    Hsv p = hsvImg[y, x];
                    int h = (int)p.Hue;
                    int s = (int)p.Satuation;
                    int v = (int)p.Value;
                    int index = series.Points.AddXY(h, s);
                    series.Points[index].YValues = new double[] { s, v };
                    series.Points[index].Color = Color.FromArgb(v, v, v);
                    series.Points[index].MarkerSize = Math.Max(3, v / 15);
                    series.Points[index].Tag = img[y, x];
                }
            }

            chart.Legends.Clear();
            Label infoLabel = CreateColorInfoLabel();
            MakeChartInteractive(chart, area, (Bgr color) =>
            {
                int r = (int)color.Red;
                int g = (int)color.Green;
                int b = (int)color.Blue;
                Image<Bgr, byte> pixel = new Image<Bgr, byte>(1, 1);
                pixel[0, 0] = color;
                Image<Hsv, byte> hsvPix = pixel.Convert<Hsv, byte>();
                Hsv hsv = hsvPix[0, 0];
                int h = (int)(hsv.Hue * 2.0);
                int s = (int)(hsv.Satuation * 100.0 / 255.0);
                int v = (int)(hsv.Value * 100.0 / 255.0);
                s = Clamp(s, 0, 100);
                v = Clamp(v, 0, 100);
                Image<Lab, byte> labPix = pixel.Convert<Lab, byte>();
                Lab lab = labPix[0, 0];
                var cmyk = RgbToCmyk(r, g, b);
                Mat yuvMat = new Mat();
                CvInvoke.CvtColor(pixel, yuvMat, ColorConversion.Bgr2Yuv);
                Image<Gray, byte>[] yuvCh = yuvMat.ToImage<Bgr, byte>().Split();
                int Y = yuvCh[0].Data[0, 0, 0];
                int U = yuvCh[1].Data[0, 0, 0];
                int V = yuvCh[2].Data[0, 0, 0];
                yuvCh[0].Dispose(); yuvCh[1].Dispose(); yuvCh[2].Dispose();
                Image<Ycc, byte> yccPix = pixel.Convert<Ycc, byte>();
                Ycc ycc = yccPix[0, 0];
                pixel.Dispose();
                infoLabel.Text =
                    $"RGB → ({r}, {g}, {b})" + Environment.NewLine +
                    $"HSV → ({h}°, {s}%, {v}%)" + Environment.NewLine +
                    $"LAB → (L:{(int)lab.X}, a:{(int)lab.Y}, b:{(int)lab.Z})" + Environment.NewLine +
                    $"YCbCr → (Y:{(int)ycc.Y}, Cr:{(int)ycc.Cr}, Cb:{(int)ycc.Cb})" + Environment.NewLine +
                    $"CMYK → (C:{cmyk.C}%, M:{cmyk.M}%, Y:{cmyk.Y}%, K:{cmyk.K}%)" + Environment.NewLine +
                    $"YUV → (Y:{Y}, U:{U}, V:{V})";
            });

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 120;
            panel.Controls.Add(infoLabel);
            f.Controls.Add(chart);
            f.Controls.Add(panel);
            f.ShowDialog();
        }

        private void ShowLABSpace(Image<Bgr, byte> img)
        {
            Image<Lab, byte> labImg = img.Convert<Lab, byte>();
            Form f = new Form();
            f.Text = "3D LAB Space";
            f.Width = 900;
            f.Height = 750;

            ChartArea area;
            Chart chart = Create3DChart("LAB", "L", "A", out area);
            Series series = chart.Series[0];

            for (int y = 0; y < labImg.Height; y += 3)
            {
                for (int x = 0; x < labImg.Width; x += 3)
                {
                    Lab p = labImg[y, x];
                    int l = (int)p.X;
                    int a = (int)p.Y;
                    int b = (int)p.Z;
                    int index = series.Points.AddXY(l, a);
                    series.Points[index].YValues = new double[] { a, b };
                    series.Points[index].Color = Color.FromArgb(l, l, l);
                    series.Points[index].MarkerSize = Math.Max(3, b / 15);
                    series.Points[index].Tag = img[y, x];
                }
            }

            chart.Legends.Clear();
            Label infoLabel = CreateColorInfoLabel();
            MakeChartInteractive(chart, area, (Bgr color) =>
            {
                int r = (int)color.Red;
                int g = (int)color.Green;
                int b = (int)color.Blue;
                Image<Bgr, byte> pixel = new Image<Bgr, byte>(1, 1);
                pixel[0, 0] = color;
                Image<Hsv, byte> hsvPix = pixel.Convert<Hsv, byte>();
                Hsv hsv = hsvPix[0, 0];
                int h = (int)(hsv.Hue * 2.0);
                int s = (int)(hsv.Satuation * 100.0 / 255.0);
                int v = (int)(hsv.Value * 100.0 / 255.0);
                s = Clamp(s, 0, 100);
                v = Clamp(v, 0, 100);
                Image<Lab, byte> labPix = pixel.Convert<Lab, byte>();
                Lab lab = labPix[0, 0];
                var cmyk = RgbToCmyk(r, g, b);
                Mat yuvMat = new Mat();
                CvInvoke.CvtColor(pixel, yuvMat, ColorConversion.Bgr2Yuv);
                Image<Gray, byte>[] yuvCh = yuvMat.ToImage<Bgr, byte>().Split();
                int Y = yuvCh[0].Data[0, 0, 0];
                int U = yuvCh[1].Data[0, 0, 0];
                int V = yuvCh[2].Data[0, 0, 0];
                yuvCh[0].Dispose(); yuvCh[1].Dispose(); yuvCh[2].Dispose();
                Image<Ycc, byte> yccPix = pixel.Convert<Ycc, byte>();
                Ycc ycc = yccPix[0, 0];
                pixel.Dispose();
                infoLabel.Text =
                    $"RGB → ({r}, {g}, {b})" + Environment.NewLine +
                    $"HSV → ({h}°, {s}%, {v}%)" + Environment.NewLine +
                    $"LAB → (L:{(int)lab.X}, a:{(int)lab.Y}, b:{(int)lab.Z})" + Environment.NewLine +
                    $"YCbCr → (Y:{(int)ycc.Y}, Cr:{(int)ycc.Cr}, Cb:{(int)ycc.Cb})" + Environment.NewLine +
                    $"CMYK → (C:{cmyk.C}%, M:{cmyk.M}%, Y:{cmyk.Y}%, K:{cmyk.K}%)" + Environment.NewLine +
                    $"YUV → (Y:{Y}, U:{U}, V:{V})";
            });

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 120;
            panel.Controls.Add(infoLabel);
            f.Controls.Add(chart);
            f.Controls.Add(panel);
            f.ShowDialog();
        }

        private void ShowYCbCrSpace(Image<Bgr, byte> img)
        {
            Image<Ycc, byte> yccImg = img.Convert<Ycc, byte>();
            Form f = new Form();
            f.Text = "3D YCbCr Space";
            f.Width = 900;
            f.Height = 750;

            ChartArea area;
            Chart chart = Create3DChart("YCbCr", "Y", "Cr", out area);
            Series series = chart.Series[0];

            for (int y = 0; y < yccImg.Height; y += 3)
            {
                for (int x = 0; x < yccImg.Width; x += 3)
                {
                    Ycc p = yccImg[y, x];
                    int Y = (int)p.Y;
                    int Cr = (int)p.Cr;
                    int Cb = (int)p.Cb;
                    int index = series.Points.AddXY(Y, Cr);
                    series.Points[index].YValues = new double[] { Cr, Cb };
                    series.Points[index].Color = Color.FromArgb(Y, Y, Y);
                    series.Points[index].MarkerSize = Math.Max(3, Cb / 15);
                    series.Points[index].Tag = img[y, x];
                }
            }

            chart.Legends.Clear();
            Label infoLabel = CreateColorInfoLabel();
            MakeChartInteractive(chart, area, (Bgr color) =>
            {
                int r = (int)color.Red;
                int g = (int)color.Green;
                int b = (int)color.Blue;
                Image<Bgr, byte> pixel = new Image<Bgr, byte>(1, 1);
                pixel[0, 0] = color;
                Image<Hsv, byte> hsvPix = pixel.Convert<Hsv, byte>();
                Hsv hsv = hsvPix[0, 0];
                int h = (int)(hsv.Hue * 2.0);
                int s = (int)(hsv.Satuation * 100.0 / 255.0);
                int v = (int)(hsv.Value * 100.0 / 255.0);
                s = Clamp(s, 0, 100);
                v = Clamp(v, 0, 100);
                Image<Lab, byte> labPix = pixel.Convert<Lab, byte>();
                Lab lab = labPix[0, 0];
                var cmyk = RgbToCmyk(r, g, b);
                Mat yuvMat = new Mat();
                CvInvoke.CvtColor(pixel, yuvMat, ColorConversion.Bgr2Yuv);
                Image<Gray, byte>[] yuvCh = yuvMat.ToImage<Bgr, byte>().Split();
                int Y = yuvCh[0].Data[0, 0, 0];
                int U = yuvCh[1].Data[0, 0, 0];
                int V = yuvCh[2].Data[0, 0, 0];
                yuvCh[0].Dispose(); yuvCh[1].Dispose(); yuvCh[2].Dispose();
                Image<Ycc, byte> yccPix = pixel.Convert<Ycc, byte>();
                Ycc ycc = yccPix[0, 0];
                pixel.Dispose();
                infoLabel.Text =
                    $"RGB → ({r}, {g}, {b})" + Environment.NewLine +
                    $"HSV → ({h}°, {s}%, {v}%)" + Environment.NewLine +
                    $"LAB → (L:{(int)lab.X}, a:{(int)lab.Y}, b:{(int)lab.Z})" + Environment.NewLine +
                    $"YCbCr → (Y:{(int)ycc.Y}, Cr:{(int)ycc.Cr}, Cb:{(int)ycc.Cb})" + Environment.NewLine +
                    $"CMYK → (C:{cmyk.C}%, M:{cmyk.M}%, Y:{cmyk.Y}%, K:{cmyk.K}%)" + Environment.NewLine +
                    $"YUV → (Y:{Y}, U:{U}, V:{V})";
            });

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 120;
            panel.Controls.Add(infoLabel);
            f.Controls.Add(chart);
            f.Controls.Add(panel);
            f.ShowDialog();
        }

        private void ShowCMYKSpace(Image<Bgr, byte> img)
        {
            Form f = new Form();
            f.Text = "3D CMY Space (from CMYK)";
            f.Width = 900;
            f.Height = 750;

            ChartArea area;
            Chart chart = Create3DChart("CMY", "Cyan", "Magenta", out area);
            Series series = chart.Series[0];

            for (int y = 0; y < img.Height; y += 3)
            {
                for (int x = 0; x < img.Width; x += 3)
                {
                    Bgr p = img[y, x];
                    int r = (int)p.Red;
                    int g = (int)p.Green;
                    int b = (int)p.Blue;

                    float R = r / 255f;
                    float G = g / 255f;
                    float B = b / 255f;
                    float K = 1 - Math.Max(Math.Max(R, G), B);
                    float C, M, Y;
                    if (K < 1f)
                    {
                        C = (1f - R - K) / (1f - K);
                        M = (1f - G - K) / (1f - K);
                        Y = (1f - B - K) / (1f - K);
                    }
                    else { C = 0f; M = 0f; Y = 0f; }

                    int cVal = (int)(C * 255);
                    int mVal = (int)(M * 255);
                    int yVal = (int)(Y * 255);

                    int index = series.Points.AddXY(cVal, mVal);
                    series.Points[index].YValues = new double[] { mVal, yVal };
                    series.Points[index].Color = Color.FromArgb(r, g, b);
                    series.Points[index].MarkerSize = Math.Max(3, yVal / 15);
                    series.Points[index].Tag = p;
                }
            }

            chart.Legends.Clear();
            Label infoLabel = CreateColorInfoLabel();
            MakeChartInteractive(chart, area, (Bgr color) =>
            {
                int r = (int)color.Red;
                int g = (int)color.Green;
                int b = (int)color.Blue;
                Image<Bgr, byte> pixel = new Image<Bgr, byte>(1, 1);
                pixel[0, 0] = color;
                Image<Hsv, byte> hsvPix = pixel.Convert<Hsv, byte>();
                Hsv hsv = hsvPix[0, 0];
                int h = (int)(hsv.Hue * 2.0);
                int s = (int)(hsv.Satuation * 100.0 / 255.0);
                int v = (int)(hsv.Value * 100.0 / 255.0);
                s = Clamp(s, 0, 100);
                v = Clamp(v, 0, 100);
                Image<Lab, byte> labPix = pixel.Convert<Lab, byte>();
                Lab lab = labPix[0, 0];
                var cmyk = RgbToCmyk(r, g, b);
                Mat yuvMat = new Mat();
                CvInvoke.CvtColor(pixel, yuvMat, ColorConversion.Bgr2Yuv);
                Image<Gray, byte>[] yuvCh = yuvMat.ToImage<Bgr, byte>().Split();
                int Y = yuvCh[0].Data[0, 0, 0];
                int U = yuvCh[1].Data[0, 0, 0];
                int V = yuvCh[2].Data[0, 0, 0];
                yuvCh[0].Dispose(); yuvCh[1].Dispose(); yuvCh[2].Dispose();
                Image<Ycc, byte> yccPix = pixel.Convert<Ycc, byte>();
                Ycc ycc = yccPix[0, 0];
                pixel.Dispose();
                infoLabel.Text =
                    $"RGB → ({r}, {g}, {b})" + Environment.NewLine +
                    $"HSV → ({h}°, {s}%, {v}%)" + Environment.NewLine +
                    $"LAB → (L:{(int)lab.X}, a:{(int)lab.Y}, b:{(int)lab.Z})" + Environment.NewLine +
                    $"YCbCr → (Y:{(int)ycc.Y}, Cr:{(int)ycc.Cr}, Cb:{(int)ycc.Cb})" + Environment.NewLine +
                    $"CMYK → (C:{cmyk.C}%, M:{cmyk.M}%, Y:{cmyk.Y}%, K:{cmyk.K}%)" + Environment.NewLine +
                    $"YUV → (Y:{Y}, U:{U}, V:{V})";
            });

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 120;
            panel.Controls.Add(infoLabel);
            f.Controls.Add(chart);
            f.Controls.Add(panel);
            f.ShowDialog();
        }

        private void ShowYUVSpace(Image<Bgr, byte> img)
        {
            Image<Bgr, byte> yuvImg = img.Convert<Bgr, byte>();
            Mat yuvMat = new Mat();
            CvInvoke.CvtColor(img, yuvMat, ColorConversion.Bgr2Yuv);
            Image<Bgr, byte> yuvImage = yuvMat.ToImage<Bgr, byte>();
            Form f = new Form();
            f.Text = "3D YUV Space";
            f.Width = 900;
            f.Height = 750;

            ChartArea area;
            Chart chart = Create3DChart("YUV", "Y", "U", out area);
            Series series = chart.Series[0];

            for (int y = 0; y < yuvImage.Height; y += 3)
            {
                for (int x = 0; x < yuvImage.Width; x += 3)
                {
                    Bgr p = yuvImage[y, x];
                    int Y = (int)p.Blue;
                    int U = (int)p.Green;
                    int V = (int)p.Red;
                    int index = series.Points.AddXY(Y, U);
                    series.Points[index].YValues = new double[] { U, V };
                    series.Points[index].Color = Color.FromArgb(Y, Y, Y);
                    series.Points[index].MarkerSize = Math.Max(3, V / 15);
                    series.Points[index].Tag = img[y, x];
                }
            }

            chart.Legends.Clear();
            Label infoLabel = CreateColorInfoLabel();
            MakeChartInteractive(chart, area, (Bgr color) =>
            {
                int r = (int)color.Red;
                int g = (int)color.Green;
                int b = (int)color.Blue;
                Image<Bgr, byte> pixel = new Image<Bgr, byte>(1, 1);
                pixel[0, 0] = color;
                Image<Hsv, byte> hsvPix = pixel.Convert<Hsv, byte>();
                Hsv hsv = hsvPix[0, 0];
                int h = (int)(hsv.Hue * 2.0);
                int s = (int)(hsv.Satuation * 100.0 / 255.0);
                int v = (int)(hsv.Value * 100.0 / 255.0);
                s = Clamp(s, 0, 100);
                v = Clamp(v, 0, 100);
                Image<Lab, byte> labPix = pixel.Convert<Lab, byte>();
                Lab lab = labPix[0, 0];
                var cmyk = RgbToCmyk(r, g, b);
                Mat yuvMat2 = new Mat();
                CvInvoke.CvtColor(pixel, yuvMat2, ColorConversion.Bgr2Yuv);
                Image<Gray, byte>[] yuvCh = yuvMat2.ToImage<Bgr, byte>().Split();
                int Y = yuvCh[0].Data[0, 0, 0];
                int U = yuvCh[1].Data[0, 0, 0];
                int V = yuvCh[2].Data[0, 0, 0];
                yuvCh[0].Dispose(); yuvCh[1].Dispose(); yuvCh[2].Dispose();
                Image<Ycc, byte> yccPix = pixel.Convert<Ycc, byte>();
                Ycc ycc = yccPix[0, 0];
                pixel.Dispose();
                infoLabel.Text =
                    $"RGB → ({r}, {g}, {b})" + Environment.NewLine +
                    $"HSV → ({h}°, {s}%, {v}%)" + Environment.NewLine +
                    $"LAB → (L:{(int)lab.X}, a:{(int)lab.Y}, b:{(int)lab.Z})" + Environment.NewLine +
                    $"YCbCr → (Y:{(int)ycc.Y}, Cr:{(int)ycc.Cr}, Cb:{(int)ycc.Cb})" + Environment.NewLine +
                    $"CMYK → (C:{cmyk.C}%, M:{cmyk.M}%, Y:{cmyk.Y}%, K:{cmyk.K}%)" + Environment.NewLine +
                    $"YUV → (Y:{Y}, U:{U}, V:{V})";
            });

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 120;
            panel.Controls.Add(infoLabel);
            f.Controls.Add(chart);
            f.Controls.Add(panel);
            f.ShowDialog();
        }

        // ==================== Real 3D Custom Rendering ====================
        public enum SpaceShape { Cube, Cylinder, Cartesian }
        public class Point3D
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
            public Color Color { get; set; }
        }

        public class Viewer3D : Form
        {
            private List<Point3D> points;
            private SpaceShape shape;
            private double yaw = 0.5;
            private double pitch = 0.5;
            private Point lastMousePos;
            private bool isMouseDown = false;
            private bool hasMoved = false;
            private double scale = 2.0;
            private Label infoLabel;
            private const int MOVE_THRESHOLD = 3;

            public Viewer3D(List<Point3D> points, SpaceShape shape, string title)
            {
                this.points = points;
                this.shape = shape;
                this.Text = title;
                this.Width = 800;
                this.Height = 800;
                this.DoubleBuffered = true;
                this.BackColor = Color.FromArgb(20, 20, 20);

                Label instructions = new Label()
                {
                    Text = "Drag to rotate | Mouse wheel to zoom | Click point for color info",
                    ForeColor = Color.White,
                    Dock = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                this.Controls.Add(instructions);

                infoLabel = new Label()
                {
                    ForeColor = Color.White,
                    Dock = DockStyle.Bottom,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Consolas", 9, FontStyle.Regular),
                    Height = 80,
                    Text = "Click a point to see its color values in all spaces"
                };
                this.Controls.Add(infoLabel);

                this.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        isMouseDown = true;
                        hasMoved = false;
                        lastMousePos = e.Location;
                    }
                };
                this.MouseMove += (s, e) =>
                {
                    if (isMouseDown)
                    {
                        int dx = e.X - lastMousePos.X;
                        int dy = e.Y - lastMousePos.Y;
                        if (Math.Abs(dx) >= MOVE_THRESHOLD || Math.Abs(dy) >= MOVE_THRESHOLD)
                            hasMoved = true;
                        if (hasMoved)
                        {
                            yaw += dx * 0.01;
                            pitch -= dy * 0.01;
                            lastMousePos = e.Location;
                            this.Invalidate();
                        }
                    }
                };
                this.MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (!hasMoved)
                        {
                            FindClosestPoint(e.Location);
                        }
                        isMouseDown = false;
                    }
                };
                this.MouseWheel += (s, e) =>
                {
                    scale += e.Delta * 0.002;
                    if (scale < 0.1) scale = 0.1;
                    this.Invalidate();
                };
            }

            private void FindClosestPoint(Point mousePos)
            {
                double minDist = double.MaxValue;
                Point3D closest = null;

                int cx = this.Width / 2;
                int cy = this.Height / 2;
                double cosY = Math.Cos(yaw), sinY = Math.Sin(yaw);
                double cosP = Math.Cos(pitch), sinP = Math.Sin(pitch);

                foreach (var pt in points)
                {
                    double x1 = pt.X * cosY - pt.Z * sinY;
                    double z1 = pt.X * sinY + pt.Z * cosY;
                    double y2 = pt.Y * cosP - z1 * sinP;
                    int sx = cx + (int)(x1 * scale);
                    int sy = cy - (int)(y2 * scale);

                    double dx = mousePos.X - sx;
                    double dy = mousePos.Y - sy;
                    double dist = dx * dx + dy * dy;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = pt;
                    }
                }

                if (closest != null && minDist < 400)
                {
                    Color c = closest.Color;
                    int r = c.R, g = c.G, b = c.B;
                    infoLabel.Text = GetColorInfo(r, g, b);
                }
            }

            private string GetColorInfo(int r, int g, int b)
            {
                var pixel = new Image<Bgr, byte>(1, 1, new Bgr((byte)b, (byte)g, (byte)r));
                Mat hsvMat = new Mat();
                CvInvoke.CvtColor(pixel, hsvMat, ColorConversion.Bgr2Hsv);
                Hsv hsv = hsvMat.ToImage<Hsv, byte>()[0, 0];
                int h = (int)(hsv.Hue * 2.0);
                int s = (int)(hsv.Satuation * 100.0 / 255.0);
                int v = (int)(hsv.Value * 100.0 / 255.0);

                Mat labMat = new Mat();
                CvInvoke.CvtColor(pixel, labMat, ColorConversion.Bgr2Lab);
                Lab lab = labMat.ToImage<Lab, byte>()[0, 0];

                Mat yccMat = new Mat();
                CvInvoke.CvtColor(pixel, yccMat, ColorConversion.Bgr2YCrCb);
                Ycc ycc = yccMat.ToImage<Ycc, byte>()[0, 0];

                float R = r / 255f, G = g / 255f, B = b / 255f;
                float K = 1 - Math.Max(Math.Max(R, G), B);
                float C, M, Y;
                if (K < 1f)
                {
                    C = (1f - R - K) / (1f - K);
                    M = (1f - G - K) / (1f - K);
                    Y = (1f - B - K) / (1f - K);
                }
                else { C = 0f; M = 0f; Y = 0f; }
                int cVal = (int)(C * 100), mVal = (int)(M * 100), yVal = (int)(Y * 100), kVal = (int)(K * 100);

                Mat yuvMat = new Mat();
                CvInvoke.CvtColor(pixel, yuvMat, ColorConversion.Bgr2Yuv);
                Image<Gray, byte>[] yuvCh = yuvMat.ToImage<Bgr, byte>().Split();
                int Yuv = yuvCh[0].Data[0, 0, 0];
                int U = yuvCh[1].Data[0, 0, 0];
                int V = yuvCh[2].Data[0, 0, 0];
                yuvCh[0].Dispose(); yuvCh[1].Dispose(); yuvCh[2].Dispose();
                pixel.Dispose();

                return $"RGB → ({r}, {g}, {b})  " +
                       $"HSV → ({Clamp(h, 0, 360)}°, {Clamp(s, 0, 100)}%, {Clamp(v, 0, 100)}%)  " +
                       $"LAB → (L:{(int)lab.X}, a:{(int)lab.Y}, b:{(int)lab.Z})  " +
                       $"YCbCr → (Y:{(int)ycc.Y}, Cr:{(int)ycc.Cr}, Cb:{(int)ycc.Cb})  " +
                       $"CMYK → (C:{cVal}%, M:{mVal}%, Y:{yVal}%, K:{kVal}%)  " +
                       $"YUV → (Y:{Yuv}, U:{U}, V:{V})";
            }
            private int Clamp(int value, int min, int max)
            {
                if (value < min) return min;
                if (value > max) return max;
                return value;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int cx = this.Width / 2;
                int cy = this.Height / 2;

                var projected = new List<(int X, int Y, double Depth, Color C)>();
                double cosY = Math.Cos(yaw), sinY = Math.Sin(yaw);
                double cosP = Math.Cos(pitch), sinP = Math.Sin(pitch);

                foreach (var pt in points)
                {
                    double x1 = pt.X * cosY - pt.Z * sinY;
                    double z1 = pt.X * sinY + pt.Z * cosY;
                    double y2 = pt.Y * cosP - z1 * sinP;
                    double z2 = pt.Y * sinP + z1 * cosP;
                    int screenX = cx + (int)(x1 * scale);
                    int screenY = cy - (int)(y2 * scale);
                    projected.Add((screenX, screenY, z2, pt.Color));
                }
                projected = projected.OrderBy(p => p.Depth).ToList();

                Pen shapePen = new Pen(Color.White, 2);
                if (shape == SpaceShape.Cube)
                    DrawCubeWireframe(g, shapePen, cx, cy, cosY, sinY, cosP, sinP, -128, 128);
                else if (shape == SpaceShape.Cylinder)
                    DrawCylinderWireframe(g, shapePen, cx, cy, cosY, sinY, cosP, sinP, 0, 0, 255, 64);
                else if (shape == SpaceShape.Cartesian)
                    DrawAxes(g, shapePen, cx, cy, cosY, sinY, cosP, sinP, -128, 128);

                // Draw axis labels
                DrawAxisLabels(g, cx, cy, cosY, sinY, cosP, sinP, shape);

                foreach (var p in projected)
                {
                    using (Brush b = new SolidBrush(p.C))
                        g.FillEllipse(b, p.X - 3, p.Y - 3, 6, 6);
                }
            }

            private void DrawCubeWireframe(Graphics g, Pen pen, int cx, int cy, double cosY, double sinY, double cosP, double sinP, double min, double max)
            {
                double[] corners = { min, max };
                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 2; j++)
                        for (int k = 0; k < 2; k++)
                        {
                            double x = corners[i], y = corners[j], z = corners[k];
                            if (i == 0) DrawLine3D(g, pen, cx, cy, cosY, sinY, cosP, sinP, x, y, z, max, y, z);
                            if (j == 0) DrawLine3D(g, pen, cx, cy, cosY, sinY, cosP, sinP, x, y, z, x, max, z);
                            if (k == 0) DrawLine3D(g, pen, cx, cy, cosY, sinY, cosP, sinP, x, y, z, x, y, max);
                        }
            }

            private void DrawCylinderWireframe(Graphics g, Pen pen, int cx, int cy, double cosY, double sinY, double cosP, double sinP, double centerX, double centerZ, double radius, int segments)
            {
                double heightMin = -128, heightMax = 128;
                for (int i = 0; i < segments; i++)
                {
                    double angle1 = 2 * Math.PI * i / segments;
                    double angle2 = 2 * Math.PI * (i + 1) / segments;
                    double x1 = centerX + radius * Math.Cos(angle1);
                    double z1 = centerZ + radius * Math.Sin(angle1);
                    double x2 = centerX + radius * Math.Cos(angle2);
                    double z2 = centerZ + radius * Math.Sin(angle2);
                    DrawLine3D(g, pen, cx, cy, cosY, sinY, cosP, sinP, x1, heightMin, z1, x2, heightMin, z2);
                    DrawLine3D(g, pen, cx, cy, cosY, sinY, cosP, sinP, x1, heightMax, z1, x2, heightMax, z2);
                    DrawLine3D(g, pen, cx, cy, cosY, sinY, cosP, sinP, x1, heightMin, z1, x1, heightMax, z1);
                }
            }

            private void DrawAxes(Graphics g, Pen pen, int cx, int cy, double cosY, double sinY, double cosP, double sinP, double min, double max)
            {
                DrawLine3D(g, pen, cx, cy, cosY, sinY, cosP, sinP, min, 0, 0, max, 0, 0);
                DrawLine3D(g, pen, cx, cy, cosY, sinY, cosP, sinP, 0, min, 0, 0, max, 0);
                DrawLine3D(g, pen, cx, cy, cosY, sinY, cosP, sinP, 0, 0, min, 0, 0, max);
            }

            private void DrawAxisLabels(Graphics g, int cx, int cy, double cosY, double sinY, double cosP, double sinP, SpaceShape shape)
            {
                Font labelFont = new Font("Arial", 8, FontStyle.Bold);
                Brush labelBrush = Brushes.White;
                StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                if (shape == SpaceShape.Cube || shape == SpaceShape.Cartesian)
                {
                    DrawLabel(g, "X", 150, 0, 0, cx, cy, cosY, sinY, cosP, sinP, labelFont, labelBrush, sf);
                    DrawLabel(g, "Y", 0, 150, 0, cx, cy, cosY, sinY, cosP, sinP, labelFont, labelBrush, sf);
                    DrawLabel(g, "Z", 0, 0, 150, cx, cy, cosY, sinY, cosP, sinP, labelFont, labelBrush, sf);
                }
                else if (shape == SpaceShape.Cylinder)
                {
                    DrawLabel(g, "Hue (angle)", 0, 140, 0, cx, cy, cosY, sinY, cosP, sinP, labelFont, labelBrush, sf);
                    DrawLabel(g, "Saturation", 200, 0, 0, cx, cy, cosY, sinY, cosP, sinP, labelFont, labelBrush, sf);
                }
            }

            private void DrawLabel(Graphics g, string text, double x, double y, double z, int cx, int cy, double cosY, double sinY, double cosP, double sinP, Font font, Brush brush, StringFormat sf)
            {
                Point screen = Project(x, y, z, cx, cy, cosY, sinY, cosP, sinP);
                g.DrawString(text, font, brush, screen.X, screen.Y, sf);
            }

            private void DrawLine3D(Graphics g, Pen pen, int cx, int cy, double cosY, double sinY, double cosP, double sinP,
                                    double x1, double y1, double z1, double x2, double y2, double z2)
            {
                var p1 = Project(x1, y1, z1, cx, cy, cosY, sinY, cosP, sinP);
                var p2 = Project(x2, y2, z2, cx, cy, cosY, sinY, cosP, sinP);
                g.DrawLine(pen, p1, p2);
            }

            private Point Project(double x, double y, double z, int cx, int cy, double cosY, double sinY, double cosP, double sinP)
            {
                double x1 = x * cosY - z * sinY;
                double z1 = x * sinY + z * cosY;
                double y2 = y * cosP - z1 * sinP;
                return new Point(cx + (int)(x1 * scale), cy - (int)(y2 * scale));
            }
        }

        private void ShowRGBSpaceReal3D(Image<Bgr, byte> img)
        {
            var points = new List<Point3D>();
            for (int y = 0; y < img.Height; y += 4)
                for (int x = 0; x < img.Width; x += 4)
                {
                    Bgr p = img[y, x];
                    points.Add(new Point3D
                    {
                        X = p.Red - 128,
                        Y = p.Green - 128,
                        Z = p.Blue - 128,
                        Color = Color.FromArgb((int)p.Red, (int)p.Green, (int)p.Blue)
                    });
                }
            new Viewer3D(points, SpaceShape.Cube, "RGB Space - Cube").ShowDialog();
        }

        private void ShowHSVSpaceReal3D(Image<Bgr, byte> img)
        {
            var hsvImg = img.Convert<Hsv, byte>();
            var points = new List<Point3D>();
            for (int y = 0; y < hsvImg.Height; y += 4)
                for (int x = 0; x < hsvImg.Width; x += 4)
                {
                    Hsv p = hsvImg[y, x];
                    Bgr color = img[y, x];
                    double angle = p.Hue * 2.0 * Math.PI / 180.0;
                    double radius = p.Satuation;
                    double height = p.Value - 128;
                    points.Add(new Point3D
                    {
                        X = radius * Math.Cos(angle),
                        Y = height,
                        Z = radius * Math.Sin(angle),
                        Color = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue)
                    });
                }
            new Viewer3D(points, SpaceShape.Cylinder, "HSV Space - Cylinder").ShowDialog();
        }

        private void ShowLABSpaceReal3D(Image<Bgr, byte> img)
        {
            var labImg = img.Convert<Lab, byte>();
            var points = new List<Point3D>();
            for (int y = 0; y < labImg.Height; y += 4)
                for (int x = 0; x < labImg.Width; x += 4)
                {
                    Lab p = labImg[y, x];
                    Bgr color = img[y, x];
                    points.Add(new Point3D
                    {
                        X = p.Y - 128,
                        Y = p.X - 128,
                        Z = p.Z - 128,
                        Color = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue)
                    });
                }
            new Viewer3D(points, SpaceShape.Cartesian, "LAB Space - Sphere (axes)").ShowDialog();
        }

        private void ShowYCbCrSpaceReal3D(Image<Bgr, byte> img)
        {
            var yccImg = img.Convert<Ycc, byte>();
            var points = new List<Point3D>();
            for (int y = 0; y < yccImg.Height; y += 4)
                for (int x = 0; x < yccImg.Width; x += 4)
                {
                    Ycc p = yccImg[y, x];
                    Bgr color = img[y, x];
                    points.Add(new Point3D
                    {
                        X = p.Cb - 128,
                        Y = p.Y - 128,
                        Z = p.Cr - 128,
                        Color = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue)
                    });
                }
            new Viewer3D(points, SpaceShape.Cube, "YCbCr Space - Cube").ShowDialog();
        }

        private void ShowCMYKSpaceReal3D(Image<Bgr, byte> img)
        {
            var points = new List<Point3D>();
            for (int y = 0; y < img.Height; y += 4)
                for (int x = 0; x < img.Width; x += 4)
                {
                    Bgr p = img[y, x];
                    var cmyk = RgbToCmyk((int)p.Red, (int)p.Green, (int)p.Blue);
                    points.Add(new Point3D
                    {
                        X = cmyk.C - 50,
                        Y = cmyk.M - 50,
                        Z = cmyk.Y - 50,
                        Color = Color.FromArgb((int)p.Red, (int)p.Green, (int)p.Blue)
                    });
                }
            new Viewer3D(points, SpaceShape.Cube, "CMYK (CMY Projection) Space - Cube").ShowDialog();
        }

        private void ShowYUVSpaceReal3D(Image<Bgr, byte> img)
        {
            Mat yuvMat = new Mat();
            CvInvoke.CvtColor(img, yuvMat, ColorConversion.Bgr2Yuv);
            Image<Bgr, byte> yuvImg = yuvMat.ToImage<Bgr, byte>();
            var points = new List<Point3D>();
            for (int y = 0; y < yuvImg.Height; y += 4)
                for (int x = 0; x < yuvImg.Width; x += 4)
                {
                    Bgr p = yuvImg[y, x];
                    Bgr color = img[y, x];
                    points.Add(new Point3D
                    {
                        X = p.Green - 128,
                        Y = p.Blue - 128,
                        Z = p.Red - 128,
                        Color = Color.FromArgb((int)color.Red, (int)color.Green, (int)color.Blue)
                    });
                }
            new Viewer3D(points, SpaceShape.Cube, "YUV Space - Cube").ShowDialog();
        }

        private void BtnSpace_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Load image first");
                return;
            }
            ShowRGBSpace(originalImage);
            ShowHSVSpace(originalImage);
            ShowLABSpace(originalImage);
            ShowYCbCrSpace(originalImage);
            ShowCMYKSpace(originalImage);
            ShowYUVSpace(originalImage);
        }

        private void BtnReal3D_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Load image first");
                return;
            }
            ShowRGBSpaceReal3D(originalImage);
            ShowHSVSpaceReal3D(originalImage);
            ShowLABSpaceReal3D(originalImage);
            ShowYCbCrSpaceReal3D(originalImage);
            ShowCMYKSpaceReal3D(originalImage);
            ShowYUVSpaceReal3D(originalImage);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}