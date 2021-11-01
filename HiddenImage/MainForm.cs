using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HiddenImage
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            MainForm_SizeChanged(null, null);
        }

        private void btnImage1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pbImage1.Image = ImageUtility.LoadBitmapFromFile(openFileDialog.FileName);
                GenerateImage();
            }
        }

        private void btnImage2_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pbImage2.Image = ImageUtility.LoadBitmapFromFile(openFileDialog.FileName);
                GenerateImage();
            }
        }

        private void GenerateImage()
        {
            if (pbImage1.Image == null || pbImage2.Image == null)
            {
                return;
            }

            if (pbImage3.Image != null)
            {
                pbImage3.Image.Dispose();
            }

            Bitmap bmp1 = pbImage1.Image.Clone() as Bitmap;
            Bitmap bmp2 = pbImage2.Image.Clone() as Bitmap;

            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
            {
                AdjustImage(bmp1, ref bmp2);
            }
            if (bmp1.PixelFormat == PixelFormat.Format32bppArgb)
            {
                ImageUtility.ARGB2Gray(ref bmp1);
            }
            else if (bmp1.PixelFormat == PixelFormat.Format24bppRgb)
            {
                ImageUtility.RGB2Gray(ref bmp1);
            }
            if (bmp2.PixelFormat == PixelFormat.Format32bppArgb)
            {
                ImageUtility.ARGB2Gray(ref bmp2);
            }
            else if (bmp2.PixelFormat == PixelFormat.Format24bppRgb)
            {
                ImageUtility.RGB2Gray(ref bmp2);
            }

            //ImageUtility.Inverse(bmp2);

            int width = bmp1.Width;
            int height = bmp1.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);
            Bitmap bmp3 = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmp1Data = bmp1.LockBits(rect, ImageLockMode.ReadWrite, bmp1.PixelFormat);
            BitmapData bmp2Data = bmp2.LockBits(rect, ImageLockMode.ReadWrite, bmp2.PixelFormat);
            BitmapData bmp3Data = bmp3.LockBits(rect, ImageLockMode.ReadWrite, bmp3.PixelFormat);
            int src_bytes1 = bmp1Data.Stride * height;
            int src_bytes2 = bmp2Data.Stride * height;
            int src_bytes3 = bmp3Data.Stride * height;
            byte[] srcValues1 = new byte[src_bytes1];
            byte[] srcValues2 = new byte[src_bytes2];
            byte[] srcValues3 = new byte[src_bytes3];
            System.Runtime.InteropServices.Marshal.Copy(bmp1Data.Scan0, srcValues1, 0, src_bytes1);
            System.Runtime.InteropServices.Marshal.Copy(bmp2Data.Scan0, srcValues2, 0, src_bytes2);

            int mixType = rb1.Checked ? 1 : rb2.Checked ? 2 : rb3.Checked ? 3 : 0;
            if (mixType == 0)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int val1 = srcValues1[i * bmp1Data.Stride + j];
                        int val2 = srcValues2[i * bmp2Data.Stride + j];
                        srcValues3[i * bmp3Data.Stride + j * 4] = (byte)((val1 + val2) / 2);
                        srcValues3[i * bmp3Data.Stride + j * 4 + 1] = (byte)((val1 + val2) / 2);
                        srcValues3[i * bmp3Data.Stride + j * 4 + 2] = (byte)((val1 + val2) / 2);
                        srcValues3[i * bmp3Data.Stride + j * 4 + 3] = (byte)(((255 - val1) + val2) / 2);
                    }
                }
            }
            else if (mixType == 1)
            {
                for (int i = 0; i < height; i += 2)
                {
                    for (int j = 0; j < width; j++)
                    {
                        byte val = (byte)(255 - srcValues1[i * bmp1Data.Stride + j]);
                        srcValues3[i * bmp3Data.Stride + j * 4] = 0;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 1] = 0;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 2] = 0;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 3] = val;
                    }
                }

                for (int i = 1; i < height; i += 2)
                {
                    for (int j = 0; j < width; j++)
                    {
                        byte val = srcValues2[i * bmp2Data.Stride + j];
                        srcValues3[i * bmp3Data.Stride + j * 4] = 255;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 1] = 255;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 2] = 255;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 3] = val;
                    }
                }
            }
            else if (mixType == 2)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j += 2)
                    {
                        byte val = (byte)(255 - srcValues1[i * bmp1Data.Stride + j]);
                        srcValues3[i * bmp3Data.Stride + j * 4] = 0;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 1] = 0;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 2] = 0;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 3] = val;
                    }
                }

                for (int i = 0; i < height; i++)
                {
                    for (int j = 1; j < width; j += 2)
                    {
                        byte val = srcValues2[i * bmp2Data.Stride + j];
                        srcValues3[i * bmp3Data.Stride + j * 4] = 255;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 1] = 255;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 2] = 255;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 3] = val;
                    }
                }
            }
            else if (mixType == 3)
            {
                int index = 1;
                for (int i = 0; i < height; i++)
                {
                    index = 1 - index;
                    for (int j = index; j < width; j += 2)
                    {
                        byte val = (byte)(255 - srcValues1[i * bmp1Data.Stride + j]);
                        srcValues3[i * bmp3Data.Stride + j * 4] = 0;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 1] = 0;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 2] = 0;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 3] = val;
                    }
                }

                index = 0;
                for (int i = 0; i < height; i++)
                {
                    index = 1 - index;
                    for (int j = index; j < width; j += 2)
                    {
                        byte val = srcValues2[i * bmp2Data.Stride + j];
                        srcValues3[i * bmp3Data.Stride + j * 4] = 255;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 1] = 255;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 2] = 255;
                        srcValues3[i * bmp3Data.Stride + j * 4 + 3] = val;
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(srcValues3, 0, bmp3Data.Scan0, src_bytes3);
            bmp1.UnlockBits(bmp1Data);
            bmp2.UnlockBits(bmp2Data);
            bmp3.UnlockBits(bmp3Data);

            pbImage3.Image = bmp3;
        }

        private void AdjustImage(Bitmap bmp1, ref Bitmap bmp2)
        {
            Bitmap bmp = new Bitmap(bmp1.Width, bmp1.Height, bmp1.PixelFormat);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(bmp2, 0, 0, bmp.Width, bmp.Height);
            bmp2.Dispose();
            bmp2 = bmp;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (pbImage3.Image == null)
            {
                return;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                pbImage3.Image.Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }

        private void rb1_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                GenerateImage();
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            int marginH = 12;
            int marginV = 12;
            int width = (ClientSize.Width - marginH * 4) / 3;
            int height = ClientSize.Height - marginV * 4 - groupBox.Height - label1.Height;

            pbImage1.Width = width;
            pbImage1.Height = height;
            pbImage1.Location = new Point(marginH, marginV * 2 + groupBox.Height);

            pbImage2.Width = width;
            pbImage2.Height = height;
            pbImage2.Location = new Point(marginH * 2 + width, marginV * 2 + groupBox.Height);

            pbImage3.Width = width;
            pbImage3.Height = height;
            pbImage3.Location = new Point(marginH * 3 + width * 2, marginV * 2 + groupBox.Height);

            label1.Location = new Point(pbImage1.Location.X + (pbImage1.Width - label1.Width) / 2, pbImage1.Location.Y + pbImage1.Height + marginV);
            label2.Location = new Point(pbImage2.Location.X + (pbImage2.Width - label2.Width) / 2, pbImage2.Location.Y + pbImage2.Height + marginV);
            label3.Location = new Point(pbImage3.Location.X + (pbImage3.Width - label3.Width) / 2, pbImage3.Location.Y + pbImage3.Height + marginV);
        }
    }
}