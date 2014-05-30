using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseConstInit;

namespace NeuroNet
{
    public class HopfNet
    {
        public int N, numbImg;
        public float[][] cncts;
        public sbyte[] nrns;
        public char ch;

        public HopfNet(byte[][] images, int n, int colIm, int bitn, char chr)
        {
            N = n * n;
            ch = chr;
            BCI<float>.init(ref cncts, N, N);
            nrns = new sbyte[N];
            numbImg = colIm;

            Parallel.For(0, N, delegate(int i)
            {
                for (int j = 0; j < i; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < colIm; k++)
                        sum += (images[k][i * bitn] == 255 ? -1 : 1) * (images[k][j * bitn] == 255 ? -1 : 1);
                    cncts[i][j] = cncts[j][i] = sum / N;
                }
            });
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

        public Tuple<float, char> Recognize(byte[] image, int bitn)
        {
            for (int i = 0; i < N; i++)
                nrns[i] = (image[i * bitn] == 255) ? (sbyte)-1 : (sbyte)1;
            float sum, converg = 0;
            int col;
            sbyte[] netBuf = new sbyte[nrns.Count()];
            for (int i = 0; i < 10; i++)
            {
                col = 0;
                nrns.CopyTo(netBuf, 0);
                for (int j = 0; j < N; j++)
                {
                    sum = 0;
                    for (int k = 0; k < N; k++)
                        sum += netBuf[k] * cncts[j][k];
                    nrns[j] = (sum >= 0) ? (sbyte)1 : (sbyte)-1;
                    if (netBuf[j] == nrns[j])
                        col++;
                    converg += Math.Abs(netBuf[j] - nrns[j]);
                }
                if (col == N)
                    return Tuple.Create(converg / (float)Math.Sqrt(numbImg), ch);
            }
            return Tuple.Create(float.MaxValue, ch);
        }
    }
}
