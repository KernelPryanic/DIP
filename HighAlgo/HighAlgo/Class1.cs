using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ByteGraphics;

namespace HighAlgorithms
{
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
                    for (int i = 0; i < image.Length; i++)
                    	image[i] = (image[i] < BaseConstants.mxBr / 2) ? (byte)0 : (byte)BaseConstants.mxBr;
                    bufBt.UnlockBits(bufBtd);
                }
            }
            catch { return null; }
            return image;
        }
    }
}
