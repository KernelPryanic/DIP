using System;

namespace BaseConstInit
{
	public class BCI<T>
    {
        public const int mxBr = 255, byteLen = 3;

		static public void init(ref T[][] arr, int n1, int n2)
		{
			arr = new T[n1][];
			for (int i = 0; i < n1; i++)
				arr[i] = new T[n2];
		}
		
		static public void initOut(out T[][] arr, int n1, int n2)
		{
			arr = new T[n1][];
			for (int i = 0; i < n1; i++)
				arr[i] = new T[n2];
		}
		
		static public void init(ref T[][][] arr, int n1, int n2, int n3)
		{
			arr = new T[n1][][];
			for (int i = 0; i < n1; i++)
			{
				arr[i] = new T[n2][];
				for (int j = 0; j < n2; j++)
					arr[i][j] = new T[n3];
			}
		}
		
		static public void initOut(out T[][][] arr, int n1, int n2, int n3)
		{
			arr = new T[n1][][];
			for (int i = 0; i < n1; i++)
			{
				arr[i] = new T[n2][];
				for (int j = 0; j < n2; j++)
					arr[i][j] = new T[n3];
			}
		}
    }
}

