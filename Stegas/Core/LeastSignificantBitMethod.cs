using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Core
{
    public class LeastSignificantBitMethod : SteganographyMethod
    {
        public Bitmap Hide(Bitmap imageSource, string message)
        {
            byte[] data = imageSource.ToByteArray();
            // Strip the least significant bit
            int i;
            for (i = 54; i < data.Length; i++)
            {
                data[i] &= 0xfe;
            }

            i = 54;
            int argb = 0;
            foreach (char c in message.ToCharArray())
            {

                char ch = c;

                for (int b = 0; b < 8; b++)
                {
                    if (argb == 3)
                    {
                        // Skip the Alpha byte
                        i++;
                        argb = ((argb + 1) % 4);
                    }
                    data[i++] |= (byte)(ch & 0x01);
                    ch = (char)(ch >> 1);
                    argb = ((argb + 1) % 4);

                }

            }

            return data.ToBitmap();
        }

       

        public string Extract(Bitmap container)
        {
            byte[] data = container.ToByteArray();
            StringBuilder builder = new StringBuilder();
            int i = 54;
            int argb = 0;
            byte c;
            while (true)
            {
                c = 0;
                int j = 0;
                while (j < 8)
                {
                    if ((argb == 3))
                    {
                        // Skip the alpha byte
                        argb = ((argb + 1) % 4);
                        i++;
                    }

                    byte temp = data[i];
                    temp &= 0x01;
                    temp = (byte)(temp << j);
                    c |= temp;
                    argb = ((argb + 1) % 4);
                    i++;
                    j++;


                }
                // Character is completed
                if (c == 0)
                    break;
                else
                    builder.Append((char)c);

            }
            return builder.ToString();
        }
       
    }
}
