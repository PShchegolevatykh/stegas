using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Core
{
    public interface SteganographyMethod
    {
        Bitmap Hide(Bitmap imageSource, string message);
        string Extract(Bitmap container);
    }
}
