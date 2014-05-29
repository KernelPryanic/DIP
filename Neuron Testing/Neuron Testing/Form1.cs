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
        HopfNet[] hnt;
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
                    hnt = new HopfNet[dirIinfo.GetDirectories().Count()];
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

                                    bool[,] used = new bool[btd.Height, btd.Width];
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

                            hnt[idx] = new HopfNet(imagery[itemF.Name], standart, imagery[itemF.Name].Count(), byteLen, itemF.Name[0]);
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
                    bool[,] used = new bool[btd.Height, btd.Width];
                    for (int i = 0; i < btd.Stride * btd.Height; i += byteLen)
                        if (i + 2 < btd.Stride * btd.Height && im[i] == 0 && im[i + 1] == 0 && im[i + 2] == 0)
                        {
                            symb = Transformation.getSymb(ref im, ref used, i, btd.Height, btd.Width, btd.Stride);
                            if (symb != null)
                            {
                                Tuple<float, char>[] res = new Tuple<float, char>[hnt.Count()];
                                Parallel.For(0, hnt.Count(), (int j) => res[j] = hnt[j].Recognize(HighAlgo.NormalizeSqPic(symb.Item1, symb.Item4, standart), byteLen));

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
}
