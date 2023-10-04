using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ROSP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        const int RegisterSize = 12;
        static byte[] ROSP1 = new byte[RegisterSize];
        static byte[] ROSP2 = new byte[RegisterSize];
        static byte[] ROSP3 = new byte[RegisterSize];
        static byte[] Gamma = new byte[RegisterSize];
        static byte[] key = new byte[RegisterSize * 3];
        static bool DECRYPT_MODE = false;
        static void ROSP(ref byte[] imageBytes) {
            if (DECRYPT_MODE)
            {
                for (int k = 0; k < RegisterSize * 3; k++)
                {
                    if (k < 12)
                        ROSP1[k] = key[k];
                    else if (k > 11 && k < 24)
                        ROSP2[k % 12] = key[k];
                    else
                        ROSP3[k % 12] = key[k];
                }
            }
            else
            {
                Random random = new Random();
                random.NextBytes(ROSP1);
                random.NextBytes(ROSP2);
                random.NextBytes(ROSP3);
                for (int k = 0; k < RegisterSize * 3; k++)
                {
                    if (k < 12)
                        key[k] = ROSP1[k];
                    else if (k > 11 && k < 24)
                        key[k] = ROSP2[k % 12];
                    else
                        key[k] = ROSP3[k % 12];
                }
            }
            for (int i = 0; i < imageBytes.Length; i++) {




                if (i % 12 == 0)
                {
                    if ((ROSP3[11] >> 1) % 2 == 0)
                    {

                        for (int k = RegisterSize - 1; k > 0; k--)
                        {
                            ROSP1[k] = (byte)(ROSP1[k] >> 1);
                            if (ROSP1[k - 1] % 2 == 1) ROSP1[k] = (byte)(ROSP1[k] | 0b_1000_0000);
                        }
                        ROSP1[0] = (byte)(ROSP1[0] >> 1);
                        if ((((ROSP1[0] >> 7) % 2 + (ROSP1[5] >> 7) % 2 + ROSP1[10] % 2 + (ROSP1[11] >> 1) % 2) / 2) % 2 == 1) ROSP1[0] = (byte)(ROSP1[0] | 0b_1000_0000);
                    }
                    if ((ROSP1[0] >> 4) % 2 == 0)
                    {
                        for (int k = RegisterSize - 1; k > 0; k--)
                        {
                            ROSP2[k] = (byte)(ROSP2[k] >> 1);
                            if (ROSP2[k - 1] % 2 == 1) ROSP2[k] = (byte)(ROSP2[k] | 0b_1000_0000);
                        }
                        ROSP2[0] = (byte)(ROSP2[0] >> 1);
                        if ((((ROSP2[0] >> 7) % 2 + (ROSP2[5] >> 2) % 2 + ROSP2[9] % 2 + (ROSP2[11] >> 1) % 2) / 2) % 2 == 1) ROSP2[0] = (byte)(ROSP2[0] | 0b_1000_0000);
                    }
                    if ((ROSP2[0] >> 6) % 2 == 0)
                    {
                        for (int k = RegisterSize - 1; k > 0; k--)
                        {
                            ROSP3[k] = (byte)(ROSP3[k] >> 1);
                            if (ROSP3[k - 1] % 2 == 1) ROSP3[k] = (byte)(ROSP3[k] | 0b_1000_0000);
                        }
                        ROSP3[0] = (byte)(ROSP3[0] >> 1);
                        if ((((ROSP3[0] >> 7) % 2 + ROSP3[5] % 2 + (ROSP3[9] >> 4) % 2 + (ROSP3[11] >> 1) % 2) / 2) % 2 == 1) ROSP3[0] = (byte)(ROSP3[0] | 0b_1000_0000);
                    }
                    for (int k = 0; k < RegisterSize; k++)
                    {
                        Gamma[k] = (byte)(ROSP1[k] ^ ROSP2[k] ^ ROSP3[k]);
                    }
                    
                }
                imageBytes[i] = Gamma[i % RegisterSize];




            }



        }
        private byte[] ConvertBitMapToByte(Bitmap img)
        {
            byte[] Result = null;
            BitmapData bData = img.LockBits(new Rectangle(new Point(), img.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int ByteCount = bData.Stride * img.Height;
            Result = new byte[ByteCount];
            Marshal.Copy(bData.Scan0, Result, 0, ByteCount);
            img.UnlockBits(bData);
            return Result;
        }

        private Bitmap ConvertByteToBitMap(byte[] Ishod, int w, int h)
        {
            Bitmap img = new Bitmap(w, h);
            BitmapData bData = img.LockBits(new Rectangle(new Point(), img.Size), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(Ishod, 0, bData.Scan0, Ishod.Length);
            img.UnlockBits(bData);
            return img;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            pictureBox1.Image = new Bitmap("../image2.bmp");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DECRYPT_MODE = false;
            string path = "../image2.bmp";
            pictureBox1.Image = new Bitmap(path);
            Bitmap btmp = (Bitmap)pictureBox1.Image;
            byte[] imgBytes = ConvertBitMapToByte(btmp);
            ROSP(ref imgBytes);

            pictureBox1.Image = ConvertByteToBitMap(imgBytes, pictureBox1.Image.Width, pictureBox1.Image.Height);
        }


        private void button3_Click(object sender, EventArgs e)
        {
            DECRYPT_MODE = true;
            Bitmap btmp = (Bitmap)pictureBox1.Image;
            byte[] imgBytes = ConvertBitMapToByte(btmp);
            ROSP(ref imgBytes);
            pictureBox1.Image = ConvertByteToBitMap(imgBytes, pictureBox1.Image.Width, pictureBox1.Image.Height);

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
