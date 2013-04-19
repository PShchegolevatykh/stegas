using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace Core
{
    public static class StegasExtensions
    {
        public static byte[] ToByteArray(this Bitmap imageSource)
        {
            MemoryStream memoryStream = new MemoryStream();
            imageSource.Save(memoryStream, ImageFormat.Bmp);
            return memoryStream.ToArray();
        }

        public static Bitmap ToBitmap(this byte[] imageSource)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
            Bitmap bitmap = (Bitmap)tc.ConvertFrom(imageSource);
            return bitmap;
            //MemoryStream memoryStream = new MemoryStream(imageSource);
            //Bitmap bitmap = new Bitmap(memoryStream);
            //return bitmap;
        }

        public static Stream ToStreamWithPrependedLength(this string text)
        {
            BinaryWriter messageWriter = new BinaryWriter(new MemoryStream());
            messageWriter.Write((byte)text.Length);
            messageWriter.Write(Encoding.Default.GetBytes(text));
            messageWriter.Seek(0, SeekOrigin.Begin);
            return messageWriter.BaseStream;
        }

        public static string ToStringRepresentation(this Stream stream)
        {
            StreamReader messageReader = new StreamReader(stream, Encoding.Default);
            return messageReader.ReadToEnd();    
        }
    }
}
