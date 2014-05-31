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
using MatrixOperations;
using BaseConstInit;

namespace KernelNeuroNet
{
	public class KernelNet
	{
		float[][] cnts;
	
		public KernelNet(byte[][] images, int n, int fn, int bitn, char chr)
		{
			byte[][] expImages = new byte[images.GetLength(0)][];
			Array.Copy(images, expImages, 0);
			
			for (int i = 0; i < images.GetLength(0); i++)
				expImages[i] = DimUp(images[i], fn);
				
			BCI<float>.init(ref cnts, fn, fn);
			
			for (int i = 0; i < fn; i++)
				for (int j = 0; j < fn; j++)
					cnts[i][j] = K(expImages[i], expImages[j]);
		}
		
		/*public Tuple<float, char> Recognize()
		{
			
		}*/
		
		byte[] DimUp(byte[] v, int fn)
		{
			if (fn < v.Length)
				throw new Exception("Ok, but I'll not do this :) New dimension is lower than native.");
			if (fn > Math.Pow(2, v.Length))
				throw new Exception("Can't get so high dimension :)");
			return DoDimUp(v, ref fn);
		}
		
		byte[] DoDimUp(byte[] v, ref int n)
		{
			int fild = 0;
			byte[] res = new byte[Math.Min((int)(Math.Pow(2, v.Length) - 1), n)];
			Array.Copy(v, res, v.Length);
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
	}
}

