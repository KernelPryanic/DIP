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
using System.Threading;
using ByteGraphics;
using NeuroNet;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using HighAlgorithms;
using BaseConstInit;
using KernelNeuroNet;
using MatrixOperations;

namespace Neuron_Testing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        void changes()
        {
            pictureBox1.Location = new Point((int)(this.Width / 24.0), this.Height / 15);
            pictureBox1.Size = new Size((int)(this.Width / 1.1), (int)(this.Height / 1.3));
            button1.Size = button2.Size = new Size(this.Width / 4, this.Height / 16);
            button1.Font = button2.Font = new System.Drawing.Font(FontFamily.GenericSerif, button1.Height / 3);
            button1.Location = new Point(pictureBox1.Location.X, this.Height - button1.Height * 2);
            button2.Location = new Point(pictureBox1.Location.X + pictureBox1.Width - button2.Width, this.Height - button2.Height * 2);
            textBox1.Size = new Size((int)(button1.Width * 1.5), textBox1.Height);
            textBox1.Location = new Point((int)(this.Width / 3.2), button1.Location.Y);
            extFilter = new String[] { ".jpg", ".bmp", ".png" };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            changes();
        }

        DirectoryInfo dirIinfo;
        String[] extFilter;
        Dictionary<String, byte[][]> imagery;
        KernelNet[] knt;
        const int standart = 17;

        private void button1_Click(object sender, EventArgs e)
        {
            int idx = 0;
            imagery = new Dictionary<String, byte[][]>();
            byte[] bufIm;
            Tuple<byte[], Point, Point, int> t = null;
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    dirIinfo = new DirectoryInfo(fbd.SelectedPath);
                    knt = new KernelNet[dirIinfo.GetDirectories().Count()];
                    long t1 = DateTime.Now.Ticks;
                    foreach (var itemF in dirIinfo.GetDirectories())
                    {
                        int c = 0;
                        imagery.Add(itemF.Name, new byte[itemF.GetFiles().Count()][]);
                        Bitmap bt = null;
                        try
                        {
                            foreach (var item in itemF.GetFiles())
                            {
                                if (Array.Find(extFilter, (String s) => s == item.Extension) != null)
                                {
                                    bt = (Bitmap)Bitmap.FromFile(item.FullName);

                                    BitmapData btd = bt.LockBits(new Rectangle(0, 0, bt.Width, bt.Height), ImageLockMode.ReadWrite, bt.PixelFormat);
                                    bufIm = Transformation.Do_BinarisationOts(ref btd);
                                    bt.UnlockBits(btd);

                                    bool[][] used = null;
                                    BCI.init<bool>(ref used, btd.Height, btd.Width);
                                    
                                    for (int i = 0; i < bufIm.Length; i += byteLen)
                                        if (i - btd.Stride * (i / btd.Stride) < byteLen * btd.Width && bufIm[i] == 0 && bufIm[i + 1] == 0 && bufIm[i + 2] == 0)
                                        {
                                            t = Transformation.getSymb(ref bufIm, ref used, i, btd.Height, btd.Width, btd.Stride);
                                            break;
                                        }

                                    if (t != null)
                                    {
                                        imagery[itemF.Name][c] = HighAlgo.NormalizeSqPic(t.Item1, t.Item4, standart);
                                        c++;
                                    }
	                                }
                            }

                            knt[idx] = new KernelNet(imagery[itemF.Name], standart, standart + 1, byteLen, itemF.Name[0]);
                            idx++;
                        }
                        catch { }
                    }
                    textBox1.Text = ((DateTime.Now.Ticks - t1) / 10000.0).ToString();
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            changes();
        }

        const int byteLen = 3;

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Bitmap bt = (Bitmap)Bitmap.FromFile(ofd.FileName);
                    byte[] im = new byte[bt.Width * bt.Width * byteLen];

                    pictureBox1.Image = bt; 
                    BitmapData btd = bt.LockBits(new Rectangle(0, 0, bt.Width, bt.Height), ImageLockMode.ReadWrite, bt.PixelFormat);

                    Transformation.Do_BinarisationNiblack(ref btd, 3);
                    Transformation.Do_My_Erosion_Dilatation(ref btd, true, 2);
                    im = Transformation.Do_My_Erosion_Dilatation(ref btd, false, 2);

                    bt.UnlockBits(btd);

                    pictureBox1.Image = bt;

                    Graphics gr = Graphics.FromImage(bt);
                    Tuple<byte[], Point, Point, int> symb = null;

                    long t1 = DateTime.Now.Ticks;
                    bool[][] used= null;
                    BCI.init<bool>(ref used, btd.Height, btd.Width);
                    
                    for (int i = 0; i < btd.Stride * btd.Height; i += byteLen)
                        if (i + 2 < btd.Stride * btd.Height && im[i] == 0 && im[i + 1] == 0 && im[i + 2] == 0)
                        {
                            symb = Transformation.getSymb(ref im, ref used, i, btd.Height, btd.Width, btd.Stride);
                            if (symb != null)
                            {
                                Tuple<float, char>[] res = new Tuple<float, char>[knt.Count()];
                                Parallel.For(0, knt.Count(), (int j) => res[j] = knt[j].Recognize(HighAlgo.NormalizeSqPic(symb.Item1, symb.Item4, standart), byteLen, 0.000000001F));

                                int y = i / btd.Stride, x = (i - ((int)(i / btd.Stride) * btd.Stride)) / byteLen;
                                gr.DrawString(res.Min().Item2.ToString(), new Font("Arial", 16), Brushes.Red, new PointF(x, y));
                            }
                            symb = null;
                        }
                    textBox1.Text = ((DateTime.Now.Ticks - t1) / 10000.0).ToString();
                    pictureBox1.Image = bt;
                }
                
            }
        }
    }
    
    public class KernelNet
	{
 		float[][] revCnts, multTr;
		byte[][] memImages;
		int N, fN;
		char ch;
	
		public KernelNet(byte[][] images, int n, int fn, int bitn, char chr)
		{
			ch = chr;
			N = n * n;
			fN = fn * fn;
			byte[][] expImages = new byte[images.Length][];
			memImages = new byte[images.Length][];
			
			for (int i = 0; i < images.GetLength(0); i++)
			{
				memImages[i] = Sify(images[i], bitn);
				expImages[i] = DimUp(memImages[i], fN);
			}
				
			float[][] cnts = null;
			BCI.init<float>(ref cnts, images.Length, images.Length);
			
			for (int i = 0; i < images.Length; i++)
				for (int j = 0; j < images.Length; j++)
					cnts[i][j] = K(expImages[i], expImages[j]);
					
			revCnts = MtrxOps.GetReverse(cnts);
			multTr = MtrxOps.GetTransp<float>(MtrxOps.GetMult(MtrxOps.GetTransp<byte>(memImages), revCnts));
		}
		
		public Tuple<float, char> Recognize(byte[] image, int bitn, float eps)
		{
			float[] z = new float[memImages.Length], x = ToFloat(Sify(image, bitn)), 
			y = new float[N], pred = new float[x.Length];
			float converg = 0, buf;
			
			while ((buf = Math.Abs(norma(x) - norma(pred))) > eps)
			{
				converg += buf;
				pred = x;
				for (int i = 0; i < memImages.Length; i++)
					z[i] = K(memImages[i], x);
				y = MtrxOps.GetMult(z, multTr);
				x = SigmoidActFunc(y);
			}
			
			return Tuple.Create(converg / (float)Math.Sqrt(memImages.Length), ch);
		}
		
		byte[] DimUp(byte[] v, int fn)
		{
			if (fn < v.Length)
				throw new Exception("Ok, but I'll not do this :) New dimension is lower than native.");
			if (fn >= Math.Pow(2, v.Length))
				throw new Exception("Can't get so high dimension :)");
			return DoDimUp(v, ref fn);
		}
		
		byte[] DoDimUp(byte[] v, ref int n)
		{
			int fild = 0;
			byte[] res = new byte[n];
			v.CopyTo(res, 0);
			n -= v.Length;
			fild += v.Length;
			
			for (int i = 0; i < v.Length; i++)
			{
				byte[] buf = new byte[Math.Min(v.Length - i - 1, n)];
				for (int j = 0; j < buf.Length; j++)
					buf[j] = (byte)(v[i] * v[i + 1 + j]);
				byte[] rec = DoDimUp(buf, ref n);
				Array.Copy(rec, 0, res, fild, rec.Length);
				fild += rec.Length;
				if (n == 0)
					return res;
			}
			return res;
		}
		
		float[] SigmoidActFunc(float[] v)
		{
			float[] res = new float[v.Length];
			for (int i = 0; i < v.Length; i++)
				res[i] = (float)Math.Exp(10 * (v[i] - 0.5)) / (float)(1 + Math.Exp(10 * (v[i] - 0.5)));
			return res;	
		}
		
		float K(byte[] v1, byte[] v2)
		{
			if (v1.Length != v2.Length)
				throw new Exception("Kernel can't apply to vectors different dimensions");
			
			float res = 0;
			for (int i = 0; i < v1.Length; i++)
				res += (float)(v1[i] * v2[i]);
				
			return res;
		}
		
		float K(byte[] v1, float[] v2)
		{
			if (v1.Length != v2.Length)
				throw new Exception("Kernel can't apply to vectors different dimensions");
			
			float res = 0;
			for (int i = 0; i < v1.Length; i++)
				res += (float)(v1[i] * v2[i]);
				
			return res;
		}
		
		byte[] Sify(byte[] image, int bitn)
		{
			byte[] res = new byte[image.Length / bitn];
			for (int i = 0; i < res.Length; i++)
				res[i] = (byte)((image[i * bitn] == 0) ? 0 : 1);
			return res;
		}
		
		float[] ToFloat(byte[] image)
		{
			float[] res = new float[image.Length];
			for (int i = 0; i < image.Length; i++)
				res[i] = (float)image[i];
			return res;
		}
		
		float norma(float[] v)
		{
			float sum = 0;
			for (int i = 0; i < v.Length; i++)
				sum += v[i] * v[i];
			return (float)Math.Sqrt(sum);
		}
	}
	
	public class HighAlgo
    {
        public static void bestSqScale(ref int[] numb, ref byte[] mult, int size, int standart, int col, byte kind)
        {
            if (size > standart)
                return;
            if (numb[size] == 0 || numb[size] > col)
            {
                numb[size] = col;
                mult[size] = kind;
            }
            bestSqScale(ref numb, ref mult, size * 2, standart, col + 1, 2);
            bestSqScale(ref numb, ref mult, size * 3, standart, col + 1, 3);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public static byte[] NormalizeSqPic(byte[] image, int size, int standart)
        {
            try
            {
                if (size < standart)
                {
                    int[] numb = new int[standart + 1];
                    byte[] mult = new byte[standart + 1];
                    Bitmap btLocal = new Bitmap(size, size, PixelFormat.Format24bppRgb);
                    BitmapData btdLocal = btLocal.LockBits(new Rectangle(0, 0, btLocal.Height, btLocal.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    int stride = btdLocal.Stride, imH = btdLocal.Height, imW = btdLocal.Width, i;

                    bestSqScale(ref numb, ref mult, size, standart, 0, 1);
                    for (i = standart; i >= 0; i--)
                        if (mult[i] != 0)
                            break;

                    while (mult[i] != 1)
                    {
                        btLocal = new Bitmap(imW * mult[i], imH * mult[i], PixelFormat.Format24bppRgb);
                        btdLocal = btLocal.LockBits(new Rectangle(0, 0, btLocal.Height, btLocal.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                        image = (mult[i] == 2) ? Transformation.Scale2x(image, imH, imW, stride) : Transformation.Scale3x(image, imH, imW, stride);
                        imH = btdLocal.Height; imW = btdLocal.Width; stride = btdLocal.Stride;
                        i /= mult[i];
                    }
                    image = Transformation.toStandart(ref image, imH, imW, stride, standart, standart);
                }
                else
                {
                    Bitmap bufBt = new Bitmap(size, size, PixelFormat.Format24bppRgb);
                    BitmapData bufBtd = bufBt.LockBits(new Rectangle(0, 0, bufBt.Width, bufBt.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    Marshal.Copy(image, 0, bufBtd.Scan0, image.Length);
                    bufBt.UnlockBits(bufBtd);

                    bufBt = Transformation.Do_Reduction(ref bufBt);

                    bufBtd = bufBt.LockBits(new Rectangle(0, 0, bufBt.Width, bufBt.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    image = new byte[bufBtd.Stride * bufBtd.Height];
                    Marshal.Copy(bufBtd.Scan0, image, 0, image.Length);
                    bufBt.UnlockBits(bufBtd);
                }
            }
            catch { return null; }
            return image;
        }
    }
}
