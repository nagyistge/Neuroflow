using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public static class HLSConvert
    {
        static double Min(double a, double b, double c)
        {
            return Math.Min(Math.Min(a, b), c);
        }

        static double Max(double a, double b, double c)
        {
            return Math.Max(Math.Max(a, b), c);
        }

        public static void RGBToHLS(byte r, byte g, byte b, out double h, out double l, out double s)
        {
            double nr = (double)r / 255.0;
            double ng = (double)g / 255.0;
            double nb = (double)b / 255.0;

            double maxc = Max(nr, ng, nb);
	        double minc = Min(nr, ng, nb);
	        l = (minc + maxc) / 2.0;
	        if (minc == maxc) 
            {
                h = 0.0;
                s = 0.0;
                return;
	        }
	        double span = (maxc - minc);
	        if (l <= 0.5) 
            {
		        s = span / (maxc + minc);
	        } 
            else 
            {
		        s = span / (2.0 - maxc - minc);
	        }
	        double rc = (maxc - nr) / span;
	        double gc = (maxc - ng) / span;
	        double bc = (maxc - nb) / span;
	        if (r == maxc) 
            {
		        h = bc - gc;
	        } 
            else if (g == maxc) 
            {
		        h = 2.0 + rc - bc;
	        } 
            else 
            {
		        h = 4.0 + gc - rc;
	        }
            h = (h / 6.0) % 1.0;
        }

        //static double V(double m1, double m2)
        //{
        //    const double ONE_SIXTH = 1.0 / 6.0;
        //    const double TWO_THIRD = 2.0 / 3.0;

        //    double hue = hue % 1.0;

        //    if hue < ONE_SIXTH {
        //        return m1 + (m2-m1)*hue*6.0
        //    }
        //    if hue < 0.5 {
        //        return m2
        //    }
        //    if hue < TWO_THIRD {
        //        return m1 + (m2-m1)*(TWO_THIRD-hue)*6.0
        //    }
        //    return m1
        //}

        /*

        // Ported from Python colorsys
        func _v(m1, m2, hue float64) float64 {
	        ONE_SIXTH := 1.0 / 6.0
	        TWO_THIRD := 2.0 / 3.0
	        hue = math.Fmod(hue, 1.0)
	        if hue < ONE_SIXTH {
		        return m1 + (m2-m1)*hue*6.0
	        }
	        if hue < 0.5 {
		        return m2
	        }
	        if hue < TWO_THIRD {
		        return m1 + (m2-m1)*(TWO_THIRD-hue)*6.0
	        }
	        return m1
        }

        // Ported from Python colorsys
        func hls_to_rgb(h, l, s float64) (float64, float64, float64) {
	        ONE_THIRD := 1.0 / 3.0
	        if s == 0.0 {
		        return l, l, l
	        }
	        var m2 float64
	        if l <= 0.5 {
		        m2 = l * (1.0 + s)
	        } else {
		        m2 = l + s - (l * s)
	        }
	        m1 := 2.0*l - m2
	        return _v(m1, m2, h+ONE_THIRD), _v(m1, m2, h), _v(m1, m2, h-ONE_THIRD)
        }
        */
    }
}
