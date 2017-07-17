using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CarolsAutoSubTitleMaker
{
    class PerceptualHash
    {
        Image sourceImage;
        public PerceptualHash(string filePath)
        {
            sourceImage = Image.FromFile(filePath);
        }
        public PerceptualHash(Bitmap bm)
        {
            sourceImage = bm;
        }
        private Image reduceSize(int width = 8,int height = 8)
        {
            Image image = sourceImage.GetThumbnailImage(width, height, () => { return false; }, IntPtr.Zero);
            return image;
        }

        private Byte[] reduceColor(Image image)
        {
            Bitmap bitmap = new Bitmap(image);
            Byte[] grayValues = new Byte[image.Width * image.Height];

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0;y < image.Height; y++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    byte grayValue = (byte)((color.R * 30 + color.G * 59 + color.B * 11) / 100);
                    grayValues[x * image.Width + y] = grayValue;
                }
            }
            return grayValues;
        }

        private Byte calcAverage(byte[] values)
        {
            int sum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                sum += (int)values[i];
            }
            return Convert.ToByte(sum / values.Length);
        }

        private String ComputeBits(byte[] values,byte averageValue)
        {
            char[] result = new char[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if(values[i] < averageValue)
                {
                    result[i] = '0';
                }
                else
                {
                    result[i] = '1';
                }
            }
            return new String(result);
        }

        public String GetHash()
        {
            Image image = reduceSize();
            Byte[] grayValues = reduceColor(image);
            byte average = calcAverage(grayValues);
            String result = ComputeBits(grayValues, average);
            return result;
        }

        public static Int32 CalcSimilarDegree(string a,string b)
        {
            if (a.Length != b.Length)
            {
                throw new ArgumentException("参数的长度不同。");
            }
            int count = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    count++;
                }
            }
            return count;
        }
    }
}
