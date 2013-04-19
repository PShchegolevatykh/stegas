using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Core
{
    public class PaletteSortingMethod : SteganographyMethod
    {
        public Bitmap Hide(Bitmap imageSource, string message)
        {
            //list the palette entries an integer values
            int[] colors = new int[imageSource.Palette.Entries.Length];
            for (int n = 0; n < colors.Length; n++)
            {
                colors[n] = imageSource.Palette.Entries[n].ToArgb();
            }

            //initialize empty list for the resulting palette
            ArrayList resultList = new ArrayList(colors.Length);

            //initialize and fill list for the sorted palette
            ArrayList originalList = new ArrayList(colors);
            originalList.Sort();

            //initialize list for the mapping of old indices to new indices
            SortedList oldIndexToNewIndex = new SortedList(colors.Length);

            Random random = new Random();
            bool messageBit = false;
            Stream messageStream = message.ToStreamWithPrependedLength();
            int messageByte = messageStream.ReadByte();
            int listElementIndex = 0;

            //for each byte of the message
            while (messageByte > -1)
            {
                //for each bit
                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {
                    //decide which color is going to be the next one in the new palette
                    messageBit = ((messageByte & (1 << bitIndex)) > 0) ? true : false;
                    if (messageBit)
                    {
                        listElementIndex = 0;
                    }
                    else
                    {
                        listElementIndex = random.Next(1, originalList.Count);
                    }

                    //log change of index for this color
                    int originalPaletteIndex = Array.IndexOf(colors, originalList[listElementIndex]);
                    if (!oldIndexToNewIndex.ContainsKey(originalPaletteIndex))
                    {
                        //add mapping, ignore if the original palette contains more than one entry for this color
                        oldIndexToNewIndex.Add(originalPaletteIndex, resultList.Count);
                    }

                    //move the color from old palette to new palette
                    resultList.Add(originalList[listElementIndex]);
                    originalList.RemoveAt(listElementIndex);
                }

                //repeat this with the next byte of the message
                messageByte = messageStream.ReadByte();
            }

            //copy unused palette entries
            foreach (object obj in originalList)
            {
                int originalPaletteIndex = Array.IndexOf(colors, obj);
                oldIndexToNewIndex.Add(originalPaletteIndex, resultList.Count);
                resultList.Add(obj);
            }

            //create new image
            Bitmap newImage = CreateBitmap(imageSource, resultList, oldIndexToNewIndex);

            return newImage;
        }

        private Bitmap CreateBitmap(Bitmap imageSource, IList palette, SortedList oldIndexToNewIndex)
        {
            int sizeBitmapInfoHeader = 40;
            int sizeBitmapFileHeader = 14;

            BitmapData bmpData = imageSource.LockBits(new Rectangle(0, 0, imageSource.Width, imageSource.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            //size of the image data in bytes
            int imageSize = (bmpData.Height * bmpData.Stride) + (palette.Count * 4);

            //copy all pixels
            byte[] pixels = new byte[imageSize];
            Marshal.Copy(bmpData.Scan0, pixels, 0, (bmpData.Height * bmpData.Stride));

            //get the new color index for each pixel
            int pixelColorIndex;
            object tmp;
            for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                pixelColorIndex = pixels[pixelIndex];
                tmp = oldIndexToNewIndex[pixelColorIndex];
                pixels[pixelIndex] = Convert.ToByte(tmp);
            }

            BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream());

            //write bitmap file header
            binaryWriter.Write(System.Text.ASCIIEncoding.ASCII.GetBytes("BM")); //BITMAPFILEHEADER.bfType;
            binaryWriter.Write((Int32)(55 + imageSize)); //BITMAPFILEHEADER.bfSize;
            binaryWriter.Write((Int16)0); //BITMAPFILEHEADER.bfReserved1;
            binaryWriter.Write((Int16)0); //BITMAPFILEHEADER.bfReserved2;
            binaryWriter.Write(
                (Int32)(
                sizeBitmapInfoHeader
                + sizeBitmapFileHeader
                + palette.Count * 4)
                ); //BITMAPFILEHEADER.bfOffBits;

            //write bitmap info header
            binaryWriter.Write((Int32)sizeBitmapInfoHeader);
            binaryWriter.Write((Int32)imageSource.Width); //BITMAPINFOHEADER.biWidth
            binaryWriter.Write((Int32)imageSource.Height); //BITMAPINFOHEADER.biHeight
            binaryWriter.Write((Int16)1); //BITMAPINFOHEADER.biPlanes
            binaryWriter.Write((Int16)8); //BITMAPINFOHEADER.biBitCount
            binaryWriter.Write((UInt32)0); //BITMAPINFOHEADER.biCompression
            binaryWriter.Write((Int32)(bmpData.Height * bmpData.Stride) + (palette.Count * 4)); //BITMAPINFOHEADER.biSizeImage
            binaryWriter.Write((Int32)0); //BITMAPINFOHEADER.biXPelsPerMeter
            binaryWriter.Write((Int32)0); //BITMAPINFOHEADER.biYPelsPerMeter
            binaryWriter.Write((UInt32)palette.Count); //BITMAPINFOHEADER.biClrUsed
            binaryWriter.Write((UInt32)palette.Count); //BITMAPINFOHEADER.biClrImportant

            //write palette
            foreach (int color in palette)
            {
                binaryWriter.Write((UInt32)color);
            }
            //write pixels
            binaryWriter.Write(pixels);

            imageSource.UnlockBits(bmpData);

            Bitmap newImage = (Bitmap)Image.FromStream(binaryWriter.BaseStream);
            newImage.RotateFlip(RotateFlipType.RotateNoneFlipY);

            binaryWriter.Close();
            return newImage;
        }

        public string Extract(Bitmap container)
        {
            //initialize empty writer for the message
            BinaryWriter messageWriter = new BinaryWriter(new MemoryStream());

            //list the palette entries an integer values
            int[] colors = new int[container.Palette.Entries.Length];
            for (int n = 0; n < colors.Length; n++)
            {
                colors[n] = container.Palette.Entries[n].ToArgb();
            }

            //initialize list for the mapping of old indices to new indices
            SortedList oldIndexToNewIndex = new SortedList(colors.Length);

            //initialize and fill list for the carrier palette
            ArrayList carrierList = new ArrayList(colors);

            //sort the list to restore the original palette
            ArrayList originalList = new ArrayList(colors);
            originalList.Sort();
            int[] unchangeableOriginalList = (int[])originalList.ToArray(typeof(int));

            //the last palette entry holds no data - remove it

            //log change of index for this color
            int sortedPaletteIndex = Array.IndexOf(unchangeableOriginalList, (int)carrierList[carrierList.Count - 1]);
            oldIndexToNewIndex.Add(carrierList.Count - 1, sortedPaletteIndex);

            carrierList.RemoveAt(carrierList.Count - 1);

            int messageBit = 0;
            int messageBitIndex = 0;
            int messageByte = 0;
            byte messageLength = 0;
            int color;
            int carrierListIndex;

            //for each color that carries a bit of the message
            for (carrierListIndex = 0; carrierListIndex < carrierList.Count; carrierListIndex++)
            {

                //decide which bit the entry's position hides
                color = (int)carrierList[carrierListIndex];

                if (color == (int)originalList[0])
                {
                    messageBit = 1;
                }
                else
                {
                    messageBit = 0;
                }

                //log change of index for this color
                sortedPaletteIndex = Array.IndexOf(unchangeableOriginalList, color);
                oldIndexToNewIndex.Add(carrierListIndex, sortedPaletteIndex);

                //remove the color from the sorted palette
                originalList.Remove(color);

                //add the bit to the message
                messageByte += (byte)(messageBit << messageBitIndex);

                messageBitIndex++;
                if (messageBitIndex > 7)
                {
                    if (messageLength == 0)
                    {
                        //first hidden byte was the message's length
                        messageLength = (byte)messageByte;
                    }
                    else
                    {
                        //append the byte to the message
                        messageWriter.Write((byte)messageByte);
                        if (messageWriter.BaseStream.Length == messageLength)
                        {
                            //finished
                            break;
                        }
                    }
                    messageByte = 0;
                    messageBitIndex = 0;
                }
            }

            //map unused palette entries
            carrierListIndex++;
            for (; carrierListIndex < carrierList.Count; carrierListIndex++)
            {
                sortedPaletteIndex = Array.IndexOf(unchangeableOriginalList, (int)carrierList[carrierListIndex]);
                oldIndexToNewIndex.Add(carrierListIndex, sortedPaletteIndex);
            }

            //create new image
            container = CreateBitmap(container, unchangeableOriginalList, oldIndexToNewIndex);

            //return message
            messageWriter.Seek(0, SeekOrigin.Begin);
            return messageWriter.BaseStream.ToStringRepresentation();
        }
    }
}
