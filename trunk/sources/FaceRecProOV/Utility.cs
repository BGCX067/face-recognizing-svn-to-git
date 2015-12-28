using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System;

namespace MultiFaceRec
{
    class Utility
    {
        // convert bytes array to Emgu.CV.Image
        public static Image<Gray, byte> GetImageFromBytes(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            Bitmap bmp = new Bitmap(Image.FromStream(ms));
            return new Image<Gray, byte>(bmp);
        }

        // convert Emgu.CV.Image to bytes array
        public static byte[] GetBytesFromImage(Image<Gray, byte> img)
        {
            MemoryStream ms = new MemoryStream();
            img.ToBitmap().Save(ms, ImageFormat.Bmp);
            return ms.ToArray();
        }

        // tính tuổi
        public static int GetAge(DateTime dob)
        { 
            return DateTime.Now.Year - dob.Year + 1;
        }
    }
}
