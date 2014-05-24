using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace ByteGraphics
{
    public class BaseConstants
    {
        public const int mxBr = 255, byteLen = 3;
    }

    public class Transformation : BaseConstants
    {
        public static void fill(out float[,] arr, int n, int m)
        {
            arr = new float[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    arr[i, j] = float.MaxValue;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		public static byte[] Do_BinarisationGrad(ref BitmapData btd)
        {
            float tr, s1 = 0, s2 = 0;
            byte[] image;
            float[,] br;
            try
            {
                int imW = btd.Width, imH = btd.Height, stride = btd.Stride;
                image = new byte[btd.Stride * btd.Height];
                fill(out br, imH, imW);

                Marshal.Copy(btd.Scan0, image, 0, image.Length);

                Parallel.For(0, imH, delegate(int i)
                {
                    int strideY = stride * i;
					for (int j = 0; j < imW; j++)
                    {
                        int idxX1 = strideY + (j + 1) * byteLen, idxX2 = strideY + (j - 1) * byteLen, idxY1 = stride * (i + 1) + j * byteLen, idxY2 = stride * (i - 1) + j * byteLen;
                        float bufx1 = (j == imW - 1) ? 0 : ((br[i, j + 1] == float.MaxValue) ? br[i, j + 1] = (float)((image[idxX1] * 0.11 + image[idxX1 + 1] * 0.59 + image[idxX1 + 2] * 0.3) / mxBr) : br[i, j + 1]),
                            bufx2 = (j == 0) ? 0 : ((br[i, j - 1] == float.MaxValue) ? br[i, j - 1] = (float)((image[idxX2] * 0.11 + image[idxX2 + 1] * 0.59 + image[idxX2 + 2] * 0.3) / mxBr) : br[i, j - 1]),
                            bufy1 = (i == imH - 1) ? 0 : ((br[i + 1, j] == float.MaxValue) ? br[i + 1, j] = (float)((image[idxY1] * 0.11 + image[idxY1 + 1] * 0.59 + image[idxY1 + 2] * 0.3) / mxBr) : br[i + 1, j]),
                            bufy2 = (i == 0) ? 0 : ((br[i - 1, j] == float.MaxValue) ? br[i - 1, j] = (float)((image[idxY2] * 0.11 + image[idxY2 + 1] * 0.59 + image[idxY2 + 2] * 0.3) / mxBr) : br[i - 1, j]);
                        float buf = Math.Max(Math.Abs(bufx2 - bufx1), Math.Abs(bufy2 - bufy1));
                        if (br[i, j] == float.MaxValue)
                            br[i, j] = (float)((image[strideY + j * byteLen] * 0.11 + image[strideY + j * byteLen + 1] * 0.59 + image[strideY + j * byteLen + 2] * 0.3) / mxBr);
                        s1 += br[i, j] * buf;
                        s2 += buf;
                    }
				});
                tr = s1 / s2;
                //-------------------------------------------------------------------------------//
                Parallel.For(0, imH, delegate(int i)
                {
                    int strideY = stride * i;
					for (int j = 0; j < imW; j++)
                    {
                        int idx = strideY + j * byteLen;
                        if (br[i, j] > tr)
                            image[idx] = image[idx + 1] = image[idx + 2] = mxBr;
                        else
                            image[idx] = image[idx + 1] = image[idx + 2] = 0;
                    }
				});

                Marshal.Copy(image, 0, btd.Scan0, btd.Stride * btd.Height);
            }
            catch { return null; }
            return image;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public static byte[] Do_BinarisationOts(ref BitmapData btd)
        {
            byte[] image;
            float[,] br;
            float[] p = new float[mxBr + 1];
            float dispers = 0, tr = 0;
            try
            {
                int imW = btd.Width, imH = btd.Height, stride = btd.Stride;
                float addF = (float)(1.0 / imW * imH);
                image = new byte[btd.Stride * btd.Height];
                fill(out br, imH, imW);

                Marshal.Copy(btd.Scan0, image, 0, image.Length);

                Parallel.For(0, imH, (int i) =>
                {
                    int strideY = stride * i;
                    for (int j = 0; j < imW; j++)
                    {
                        br[i, j] = (float)(image[strideY + j * byteLen] * 0.11 + image[strideY + j * byteLen + 1] * 0.59 + image[strideY + j * byteLen + 2] * 0.3);
                        p[(int)(br[i, j])] += addF;
                    }
                });

                Parallel.For(0, mxBr, (int i) =>
                {
                    float w0, w1, m0, m1, bufDispers;
                    w0 = w1 = m0 = m1 = 0;
                    for (int j = 0; j <= mxBr; j++)
                        if (j < i)
                            w0 += p[j];
                        else
                            w1 += p[j];
                    for (int j = 0; j <= mxBr; j++)
                        if (j < i)
                            m0 += j * p[j] / w0;
                        else
                            m1 += j * p[j] / w1;

                    bufDispers = w0 * w1 * (m0 - m1) * (m0 - m1);
                    if (dispers < bufDispers)
                    {
                        dispers = bufDispers;
                        tr = i;
                    }
                });

                //-------------------------------------------------------------------------------//

                Parallel.For(0, imH, delegate(int i)
                {
                    int strideY = stride * i;
					for (int j = 0; j < imW; j++)
                    {
                        int idx = strideY + j * byteLen;
                        if (br[i, j] > tr)
                            image[idx] = image[idx + 1] = image[idx + 2] = mxBr;
                        else
                            image[idx] = image[idx + 1] = image[idx + 2] = 0;
                    }
				});

                Marshal.Copy(image, 0, btd.Scan0, btd.Stride * btd.Height);
            }
            catch { return null; }
            return image;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		//Убрать лишние if-else, да и вообще оптимизировать код, также автоматизировать подсчет радиуса в зависимости от размера изображения
        public static byte[] Do_BinarisationNiblack(ref BitmapData btd, int rad = 9, float factor = (float)0.0001, float downTreshold = 20, float upTreshold = 150)
        {
            float[,] br, pTr;
            byte[] image = new byte[btd.Stride * btd.Height];
            Tuple<int, float, float>[,] buffer;
            float bufIt2 = 0, bufIt3 = 0, bufBr;
            int col = 0;

            try
            {
                int imW = btd.Width, imH = btd.Height, stride = btd.Stride;
                pTr = new float[imH + 1, imW + 1];
                buffer = new Tuple<int, float, float>[imH + 1, imW + 1];
                fill(out br, imH + 1, imW + 1);

                Marshal.Copy(btd.Scan0, image, 0, image.Length);

                for (int i = 0; i <= Math.Min(rad, imH); i++)
                    for (int j = 0; j <= Math.Min(rad, imW); j++)
                    {
                        int buf = btd.Stride * i + byteLen * j;
                        if (buf + 2 < btd.Stride * imH)
                        {
                            br[i, j] = bufBr = (float)(image[buf] * 0.11 + image[buf + 1] * 0.59 + image[buf + 2] * 0.3);
                            col++;
                            bufIt2 += bufBr;
                            bufIt3 += bufBr * bufBr;
                        }
                    }

                buffer[0, 0] = Tuple.Create(col, bufIt2, bufIt3);
                bufIt2 /= col; bufIt3 /= col;
                bufIt3 = (float)Math.Sqrt(bufIt3 - bufIt2 * bufIt2);
                pTr[0, 0] = bufIt2 * (1 + factor * (bufIt3 / rad - 1)) - (float)Math.Sqrt(bufIt3);

                for (int i = 0; i < imH; i++)
                {
                    bool way;

                    for (int j = (i == 0) ? 1 : 0; j < imW; j++)
                    {
                        int buf;
                        if (j > 0)
                        {
                            col = buffer[i, j - 1].Item1; bufIt2 = buffer[i, j - 1].Item2; bufIt3 = buffer[i, j - 1].Item3;
                            way = true;
                        }
                        else
                        {
                            col = buffer[i - 1, j].Item1; bufIt2 = buffer[i - 1, j].Item2; bufIt3 = buffer[i - 1, j].Item3;
                            way = false;
                        }


                        for (int p = Math.Max(((way) ? i : j) - rad, 0); p <= Math.Min(((way) ? imH : imW) - 1, ((way) ? i : j) + rad); p++)
                        {
                            buf = (way) ? ((j + rad) * byteLen + p * btd.Stride) : ((i + rad) * btd.Stride + p * byteLen);
                            int iBif = ((way) ? p : (i + rad)), jBif = ((way) ? j + rad : p);

                            if (iBif < imH && jBif < imW)
                            {
                                br[iBif, jBif] = bufBr = (float)(image[buf] * 0.11 + image[buf + 1] * 0.59 + image[buf + 2] * 0.3);
                                bufIt2 += bufBr;
                                bufIt3 += bufBr * bufBr;
                                col++;
                            }

                            buf = (way) ? ((j - (rad + 1)) * byteLen + p * btd.Stride) : ((i - (rad + 1)) * btd.Stride + p * byteLen);
                            iBif = ((way) ? p : (i - (rad + 1))); jBif = ((way) ? j - (rad + 1) : p);

                            if (iBif >= 0 && jBif >= 0)
                            {
                                bufBr = (float)(image[buf] * 0.11 + image[buf + 1] * 0.59 + image[buf + 2] * 0.3);
                                bufIt2 -= bufBr;
                                bufIt3 -= bufBr * bufBr;
                                col--;
                            }
                        }

                        buffer[i, j] = Tuple.Create(col, bufIt2, bufIt3);
                        bufIt2 /= col; bufIt3 /= col;
                        bufIt3 = (float)Math.Sqrt(bufIt3 - bufIt2 * bufIt2);
                        pTr[i, j] = bufIt2 * (1 + factor * (bufIt3 / rad - 1)) - (float)Math.Sqrt(bufIt3);
                    }
                }

                //-------------------------------------------------------------------------------//

                Parallel.For(0, imH, (int i) =>
                {
                    int strideY = stride * i;
                    for (int j = 0; j < imW; j++)
                    {
                        int idx = strideY + j * byteLen;
                        if (br[i, j] > upTreshold)
                            image[idx] = image[idx + 1] = image[idx + 2] = 255;
                        else
                            if (br[i, j] < downTreshold)
                                image[idx] = image[idx + 1] = image[idx + 2] = 0;
                            else
                                if (br[i, j] > pTr[i, j])
                                    image[idx] = image[idx + 1] = image[idx + 2] = 255;
                                else
                                    image[idx] = image[idx + 1] = image[idx + 2] = 0;
                    }
                });

                Marshal.Copy(image, 0, btd.Scan0, btd.Stride * btd.Height);

            }
            catch { return null; }
            return image;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public static byte[] Do_Erosion_Dilatation(ref BitmapData btd, bool wat = true, int rad = 1, int col = 1)
        {
            byte bufMx = (byte)((wat) ? 0 : 255), compare = (byte)((wat) ? 255 : 0);
            byte[] image;

            try
            {
                int imH = btd.Height, imW = btd.Width, stride = btd.Stride, xR = 0, yR = 0, xD = 0, yD = 0;
                image = new byte[btd.Stride * imH];
                Tuple<int, int, int, int, byte>[,] localMx = new Tuple<int, int, int, int, byte>[imH + 1, imW + 1];

                Marshal.Copy(btd.Scan0, image, 0, image.Length);

                for (int k = 0; k < col; k++)
                {
                    for (int i = 0; i < imH; i++)
                        for (int j = 0; j < imW; j++)
                            localMx[i, j] = Tuple.Create(0, 0, 0, 0, (byte)((wat) ? 0 : 255));

                    for (int i = 0; i <= Math.Min(rad, imH); i++)
                    {
                        int strideY = i * stride;
                        for (int j = 0; j <= Math.Min(rad, imW); j++)
                        {
                            if (image[strideY + byteLen * j] == compare)
                            {
                                bufMx = compare;
                                if (yD < i)
                                {
                                    yD = i; xD = j;
                                }
                                if (xR < j)
                                {
                                    xR = j; yR = i;
                                }
                            }
                        }
                    }
                    if (bufMx == compare)
                    {
                        localMx[0, 0] = Tuple.Create(yD, xD, yR, xR, bufMx);
                        image[0] = image[1] = image[2] = compare;
                    }

                    for (int i = 0; i < imH; i++)
                    {
                        int buf;
                        bool way;

                        for (int j = (i == 0) ? 1 : 0; j < imW; j++)
                        {
                            way = (j > 0) ? true : false;

                            int iPred = (way) ? i : i - 1, jPred = ((way) ? j - 1 : j),
                                predY = (way) ? localMx[iPred, jPred].Item3 : localMx[iPred, jPred].Item1, predX = (way) ? localMx[iPred, jPred].Item4 : localMx[iPred, jPred].Item2;
                            bufMx = localMx[iPred, jPred].Item5;
                            yD = localMx[iPred, jPred].Item1; xD = localMx[iPred, jPred].Item2;
                            yR = localMx[iPred, jPred].Item3; xR = localMx[iPred, jPred].Item4;
                            bool already = false;

                            for (int p = Math.Max(((way) ? i : j) - rad, 0); p <= Math.Min(((way) ? imH : imW) - 1, ((way) ? i : j) + rad); p++)
                            {
                                buf = (way) ? (p * stride + (j - rad) * byteLen) : (p * byteLen + (i - rad) * stride);
                                int iBif = (way) ? p : (i - rad), jBif = (way) ? j - rad : p;

                                if (iBif >= 0 && jBif >= 0)
                                {
                                    if (image[buf] == compare && predY == iBif && predX == jBif && !already)
                                        bufMx = (byte)((wat) ? 0 : 255);
                                }

                                buf = (way) ? (p * stride + (rad + j) * byteLen) : (p * byteLen + (rad + i) * stride);
                                iBif = (way) ? p : (i + rad); jBif = (way) ? j + rad : p;

                                if (iBif < imH && jBif < imW)
                                {
                                    if (image[buf] == compare)
                                    {
                                        already = true;
                                        bufMx = compare;
                                        if (yD < iBif)
                                        {
                                            yD = iBif; xD = jBif;
                                        }
                                        if (xR < jBif)
                                        {
                                            xR = jBif; yR = iBif;
                                        }
                                    }
                                }
                            }
                            localMx[i, j] = Tuple.Create(yD, xD, yR, xR, bufMx);
                        }
                    }

                    //-------------------------------------------------------------------------------//

                    Parallel.For(0, imH, (int i) =>
                    {
                        int strideY = i * stride;
                        for (int j = 0; j < imW; j++)
                            if (localMx[i, j].Item5 == compare)
                            {
                                int bufIdx = strideY + j * byteLen;
                                image[bufIdx] = image[bufIdx + 1] = image[bufIdx + 2] = compare;
                            }
                    });
                }

                Marshal.Copy(image, 0, btd.Scan0, btd.Stride * btd.Height);
            }
            catch { return null; }

            return image;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public static byte[] Do_My_Erosion_Dilatation(ref BitmapData btd, bool wat = true, int rad = 1, int col = 1, int type = 1)
        {
            byte bufMx = (byte)((wat) ? 0 : 255), compare = (byte)((wat) ? 255 : 0);
            byte[] image;

            try
            {
                int imH = btd.Height, imW = btd.Width, stride = btd.Stride;
                image = new byte[btd.Stride * imH];
                int[,] local1 = new int[imH + 1, imW + 1], local2 = new int[imH + 1, imW + 1];

                Marshal.Copy(btd.Scan0, image, 0, image.Length);

                for (int k = 0; k < col; k++)
                {
                    for (int i = 0; i <= Math.Min(rad, imH); i++)
                    {
                        int strideY = i * stride;
                        for (int j = 0; j <= Math.Min(rad - i, imW - i); j++)
                        {
                            if (image[strideY + byteLen * j] == compare)
                                local1[0, 0]++;
                            else
                                local2[0, 0]++;
                        }
                    }

                    for (int i = 0; i < imH; i++)
                    {
                        int buf1, buf2;
                        bool way;
                        for (int j = (i == 0) ? 1 : 0; j < imW; j++)
                        {
                            way = (j > 0) ? true : false;

                            int iPred = (way) ? i : (i - 1), jPred = ((way) ? j - 1 : j);
                            local1[i, j] = local1[iPred, jPred];
                            local2[i, j] = local2[iPred, jPred];

                            buf1 = (way) ? (iPred * stride + jPred * byteLen - rad * byteLen) : (iPred * stride + jPred * byteLen - rad * stride);
                            if (((way) ? jPred : iPred) - rad >= 0)
                                if (image[buf1] == compare)
                                    local1[i, j]++;
                                else
                                    local2[i, j]++;

                            for (int p = 0; p <= rad; p++)
                            {
                                buf1 = (way) ? ((iPred - (rad - p)) * stride + jPred * byteLen - p * byteLen) : ((iPred - p) * stride + jPred * byteLen - (rad - p) * byteLen);
                                buf2 = (way) ? ((iPred + (rad - p)) * stride + jPred * byteLen - p * byteLen) : ((iPred - p) * stride + jPred * byteLen + (rad - p) * byteLen);

                                int i1 = (way) ? iPred - (rad - p) : (iPred - p), j1 = (way) ? jPred - p : (jPred - (rad - p)), i2 = (way) ? iPred + (rad - p) : (iPred - p), j2 = (way) ? jPred - p : ((rad - p) + jPred);

                                if (i1 >= 0 && j1 >= 0 && i1 < imH && j1 < imW && buf1 < stride * imH)
                                {
                                    if (image[buf1] == compare)
                                        local1[i, j]--;
                                    else
                                        local2[i, j]--;
                                }

                                if (i2 < imH && j2 < imW && i2 >= 0 && j2 >= 0 && buf2 < stride * imH)
                                {
                                    if (image[buf2] == compare)
                                        local1[i, j]--;
                                    else
                                        local2[i, j]--;
                                }

                                buf1 = (way) ? ((i - (rad - p)) * stride + j * byteLen + p * byteLen) : ((i + p) * stride + j * byteLen - (rad - p) * byteLen);
                                buf2 = (way) ? ((i + (rad - p)) * stride + j * byteLen + p * byteLen) : ((i + p) * stride + j * byteLen + (rad - p) * byteLen);

                                i1 = (way) ? i - (rad - p) : (i - p); j1 = (way) ? j - p : (j - (rad - p)); i2 = (way) ? i + (rad - p) : (i - p); j2 = (way) ? j - p : ((rad - p) + j);

                                if (i1 >= 0 && j1 >= 0 && i1 < imH && j1 < imW && buf1 < stride * imH)
                                {
                                    if (image[buf1] == compare)
                                        local1[i, j]++;
                                    else
                                        local2[i, j]++;
                                }

                                if (i2 < imH && j2 < imW && i2 >= 0 && j2 >= 0 && buf2 < stride * imH)
                                {
                                    if (image[buf2] == compare)
                                        local1[i, j]++;
                                    else
                                        local2[i, j]++;
                                }
                            }

                            buf1 = (way) ? (i * stride + j * byteLen + rad * byteLen) : (i * stride + j * byteLen + rad * stride);
                            if (((way) ? j : i) + rad < ((way) ? imW : imH))
                                if (image[buf1] == compare)
                                    local1[i, j]--;
                                else
                                    local2[i, j]--;
                        }
                    }

                    //-------------------------------------------------------------------------------//

                    Parallel.For(0, imH, (int i) =>
                    {
                        int strideY = i * stride;
                        for (int j = 0; j < imW; j++)
                            if ((type == 1 && local1[i, j] >= local2[i, j]) || (type == 2 && local1[i, j] > local2[i, j]) || (type == 3 && local1[i, j] >= 1))
                            {
                                int bufIdx = strideY + j * byteLen;
                                image[bufIdx] = image[bufIdx + 1] = image[bufIdx + 2] = compare;
                            }
                    });
                }

                Marshal.Copy(image, 0, btd.Scan0, btd.Stride * btd.Height);
            }
            catch { return null; }

            return image;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public static Bitmap Do_Reduction(ref Bitmap image, int newW = 17, int newH = 17)
        {
            Bitmap buf = new Bitmap(newW, newH, PixelFormat.Format24bppRgb);
            using (Graphics gr = Graphics.FromImage(buf))
            {
                gr.DrawImage(image, 0, 0, newW, newH);

                gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                gr.DrawImage(image, new RectangleF(0, 0, newW, newH), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return buf;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        static void bfs(ref byte[] image, ref bool[,] used, int y, int x, ref int uBord, ref int rBord, ref int dBord, ref int lBord, int imH, int imW, int stride)
        {
            Queue<Tuple<int, int>> qu = new Queue<Tuple<int,int>>();
            qu.Enqueue(Tuple.Create(y, x));
            used[y, x] = true;

            while (qu.Count > 0)
            {
                y = qu.Peek().Item1; x = qu.Peek().Item2;
                qu.Dequeue();
                if (y < uBord)
                    uBord = y;
                if (y > dBord)
                    dBord = y;
                if (x < lBord)
                    lBord = x;
                if (x > rBord)
                    rBord = x;

                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                        if (y + i >= 0 && y + i < imH && x + j >= 0 && x + j < imW && !used[y + i, x + j] && image[(y + i) * stride + (x + j) * byteLen] == 0)
                        {
                            qu.Enqueue(Tuple.Create(y + i, x + j));
                            used[y + i, x + j] = true;
                        }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public static Tuple<byte[], Point, Point, int> getSymb(ref byte[] image, ref bool[,] used, int idx, int imH, int imW, int stride)
        {
            byte[] resImage;
            int dr, nSize, y = idx / stride, x = (idx - ((int)(idx / stride) * stride)) / byteLen;
            bool dir;
            Point pt1, pt2;

            if (used[y, x])
                return null;

            try
            {
                int lBord = imW, rBord = -1, uBord = imH, dBord = -1;

                bfs(ref image, ref used, y, x, ref uBord, ref rBord, ref dBord, ref lBord, imH, imW, stride);

                pt1 = new Point(lBord, uBord); pt2 = new Point(rBord, dBord);
                int dx = pt2.X - pt1.X, dy = pt2.Y - pt1.Y;
                dir = (dx > dy) ? false : true;
                dr = (dir) ? dy - dx : (dx - dy);

                nSize = Math.Max(dx + ((dir) ? dr : 0), dy + ((dir) ? 0 : dr));
                Bitmap bufBtd = new Bitmap(nSize, nSize, PixelFormat.Format24bppRgb);
                BitmapData btdForStride = bufBtd.LockBits(new Rectangle(0, 0, bufBtd.Width, bufBtd.Height), ImageLockMode.ReadWrite, bufBtd.PixelFormat);

                int nStride = btdForStride.Stride, srcIdx, distIdx;
                resImage = new byte[nStride * bufBtd.Height];

                for (int i = 0; i < resImage.Length; i++)
                    resImage[i] = 255;

                for (int i = 0; i < dy; i++)
                {
                    srcIdx = stride * (i + pt1.Y) + byteLen * pt1.X;
                    distIdx = i * nStride;
                    Array.Copy(image, srcIdx, resImage, distIdx, dx * byteLen);
                }

            }
            catch { return null; }
            return Tuple.Create(resImage, pt1, pt2, nSize);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public static byte[] Scale2x(byte[] image, int imH, int imW, int stride)
        {
            byte[] nImage;

            try
            {
                Bitmap bt = new Bitmap(imW * 2, imH * 2, PixelFormat.Format24bppRgb);
                BitmapData btd = bt.LockBits(new Rectangle(0, 0, bt.Width, bt.Height), ImageLockMode.ReadWrite, bt.PixelFormat);

                int nStride = btd.Stride;
                nImage = new byte[nStride * 2 * imH];

                Parallel.For(0, imH, (int i) =>
                    {
                        for (int j = 0; j < imW; j++)
                        {
                            byte B, F, H, D, E, E0, E1, E2, E3;
                            int curIdx = (i * stride) + (j * byteLen);
                            E = image[curIdx];
                            B = (i - 1 >= 0) ? image[curIdx - stride] : E;
                            F = (j + 1 < imW) ? image[curIdx + byteLen] : E;
                            H = (i + 1 < imH) ? image[curIdx + stride] : E;
                            D = (j - 1 >= 0) ? image[curIdx - byteLen] : E;

                            if (B != H && D != F)
                            {
                                E0 = (D == B) ? D : E;
                                E1 = (B == F) ? F : E;
                                E2 = (D == H) ? D : E;
                                E3 = (H == F) ? F : E;
                            }
                            else
                                E0 = E1 = E2 = E3 = E;

                            int nIdx = (2 * i * nStride) + (2 * j * byteLen);
                            try
                            {
                                int buf = 0;
                                nImage[nIdx] = nImage[nIdx + 1] = nImage[nIdx + 2] = E0;
                                nImage[buf = nIdx + byteLen] = nImage[buf + 1] = nImage[buf + 2] = E1;
                                nImage[buf = nIdx + nStride] = nImage[buf + 1] = nImage[buf + 2] = E2;
                                nImage[buf = nIdx + nStride + byteLen] = nImage[buf + 1] = nImage[buf + 2] = E3;
                            }
                            catch { }
                        }
                    });
            }
            catch { return null; }

            return nImage;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public static byte[] Scale3x(byte[] image, int imH, int imW, int stride)
        {
            byte[] nImage;

            try
            {
                Bitmap bt = new Bitmap(imW * 3, imH * 3, PixelFormat.Format24bppRgb);
                BitmapData btd = bt.LockBits(new Rectangle(0, 0, bt.Width, bt.Height), ImageLockMode.ReadWrite, bt.PixelFormat);

                int nStride = btd.Stride, nH = 3 * imH, nW = 3 * imW;
                nImage = new byte[nStride * nH];

                Parallel.For(0, imH, (int i) =>
                    {
                        for (int j = 0; j < imW; j++)
                        {
                            byte A, B, C, D, E, F, G, H, I, E0, E1, E2, E3, E4, E5, E6, E7, E8;
                            int curIdx = (i * stride) + (j * byteLen);
                            bool b1, b2, b3, b4;
                            E = image[curIdx];
                            B = (b1 = i - 1 >= 0) ? image[curIdx - stride] : E;
                            F = (b2 = j + 1 < imW) ? image[curIdx + byteLen] : E;
                            H = (b3 = i + 1 < imH) ? image[curIdx + stride] : E;
                            D = (b4 = j - 1 >= 0) ? image[curIdx - byteLen] : E;
                            A = (b1 && b4) ? image[curIdx - stride - byteLen] : E;
                            C = (b1 && b2) ? image[curIdx - stride + byteLen] : E;
                            G = (b3 && b4) ? image[curIdx + stride - byteLen] : E;
                            I = (b3 && b2) ? image[curIdx + stride + byteLen] : E;

                            if (B != H && D != F)
                            {
                                E0 = (D == B) ? D : E;
                                E1 = ((D == B && E != C) || (B == F && E != A)) ? B : E;
                                E2 = (B == F) ? F : E;
                                E3 = ((D == B && E != G) || (D == H && E != A)) ? D : E;
                                E4 = E;
                                E5 = ((B == F && E != I) || (H == F && E != C)) ? F : E;
                                E6 = (D == H) ? D : E;
                                E7 = ((D == H && E != I) || (H == F && E != G)) ? H : E;
                                E8 = (H == F) ? F : E;
                            }
                            else
                                E0 = E1 = E2 = E3 = E4 = E5 = E6 = E7 = E8 = E;

                            int ni = 3 * i, nj = 3 * j, nIdx = (ni * nStride) + (nj * byteLen);
                            try
                            {
                                int buf = 0;
                                nImage[nIdx] = nImage[nIdx + 1] = nImage[nIdx + 2] = E0;
                                nImage[buf = nIdx + byteLen] = nImage[buf + 1] = nImage[buf + 2] = E1;
                                nImage[buf = nIdx + 2 * byteLen] = nImage[buf + 1] = nImage[buf + 2] = E2;
                                nImage[buf = nIdx + nStride] = nImage[buf + 1] = nImage[buf + 2] = E3;
                                nImage[buf = nIdx + nStride + byteLen] = nImage[buf + 1] = nImage[buf + 2] = E4;
                                nImage[buf = nIdx + nStride + 2 * byteLen] = nImage[buf + 1] = nImage[buf + 2] = E5;
                                nImage[buf = nIdx + 2 * nStride] = nImage[buf + 1] = nImage[buf + 2] = E6;
                                nImage[buf = nIdx + 2 * nStride + byteLen] = nImage[buf + 1] = nImage[buf + 2] = E7;
                                nImage[buf = nIdx + 2 * nStride + 2 * byteLen] = nImage[buf + 1] = nImage[buf + 2] = E8;
                            }
                            catch { }
                        }
                    });
            }
            catch { return null; }

            return nImage;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public static byte[] toStandart(ref byte[] image, int imH, int imW, int stride, int nH, int nW)
        {
            Bitmap bt = new Bitmap(nW, nH, PixelFormat.Format24bppRgb);
            BitmapData btd = bt.LockBits(new Rectangle(0, 0, nW, nH), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int nStride = btd.Stride;
            byte[] nImage = new byte[nStride * nH];

            for (int i = 0; i < nImage.Length; i++)
                nImage[i] = mxBr;

            for (int i = 0; i < imH; i++)
                Array.Copy(image, i * stride, nImage, i * nStride, stride);

            return nImage;
        }
    }  
}
