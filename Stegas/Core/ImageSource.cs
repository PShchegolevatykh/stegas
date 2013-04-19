using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Core
{
    public interface ImageSource
    {
        Bitmap LoadImage(string fileName);
        void SaveImage(Bitmap image, string fileName);
    }
}
