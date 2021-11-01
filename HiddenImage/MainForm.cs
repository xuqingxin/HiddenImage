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
        }

        private void btnImage1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pbImage1.Image = Image.FromFile(openFileDialog.FileName);
                GenerateImage();
            }
        }

        private void btnImage2_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pbImage2.Image = Image.FromFile(openFileDialog.FileName);
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
    }
}