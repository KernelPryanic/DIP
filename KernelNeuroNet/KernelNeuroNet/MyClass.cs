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

namespace KernelNeuroNet
{
	public class KernelNet
	{
		public KernelNet(byte[][] images, int n, int fn, int colIm, int bitn, char chr)
		{
			
		}
		
		byte[] DimUp(byte[] v, int fn)
		{
			if (fn > Math.Pow(2, v.Length))
				throw new Exception("Can't get so high dimension :)");
			byte[] res = new byte[fn];
			
			
		}
		
		byte K(byte[] v1, byte[] v2)
		{
			if (v1.Length != v2.Length)
				throw new Exception("Kernel can't apply to vectors different dimensions");
			
			byte res = 0;
			for (int i = 0; i < v1.Length; i++)
				res[i] += v1[i] * v2[i];
				
			return res;
		}
	}
}

