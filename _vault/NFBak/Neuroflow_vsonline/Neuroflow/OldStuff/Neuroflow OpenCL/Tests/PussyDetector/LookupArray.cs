using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PussyDetector
{
    public enum WormLookupDirection { MiddleToBorder, BorderToMiddle }
    
    public static class LookupArray
    {
        enum Direction { LeftToRight, TopToBottom, RightToLeft, BottomToTop }

        public static Point[] CreateSimple(int subSampleSize)
        {
            var dir = Direction.LeftToRight;
            int resultSize = subSampleSize * subSampleSize;
            Point[] result = new Point[resultSize];
            int resultIndex = 0;

            for (int y = 0; y < subSampleSize; y++)
            {
                if (dir == Direction.LeftToRight)
                {
                    for (int x = 0; x < subSampleSize; x++)
                    {
                        result[resultIndex++] = new Point(x, y);
                    }
                    dir = Direction.RightToLeft;
                }
                else
                {
                    for (int x = subSampleSize - 1; x >= 0; x--)
                    {
                        result[resultIndex++] = new Point(x, y);
                    }
                    dir = Direction.LeftToRight;
                }
            }

            return result;
        }

        public static Point[] CreateWorm(int subSampleSize, WormLookupDirection direction)
        {
            int xfrom = 0, yfrom = 0;
            int xto = subSampleSize, yto = subSampleSize;

            var dir = Direction.LeftToRight;

            int resultSize = subSampleSize * subSampleSize;
            Point[] result = new Point[resultSize];

            if (direction == WormLookupDirection.MiddleToBorder)
            {
                for (int resultIndex = resultSize - 1; resultIndex >= 0; resultIndex--)
                {
                    FillWorm(ref xfrom, ref yfrom, ref xto, ref yto, ref dir, result, resultIndex);
                }
            }
            else
            {
                for (int resultIndex = 0; resultIndex < resultSize; resultIndex++)
                {
                    FillWorm(ref xfrom, ref yfrom, ref xto, ref yto, ref dir, result, resultIndex);
                }
            }

            return result;
        }

        private static void FillWorm(ref int xfrom, ref int yfrom, ref int xto, ref int yto, ref Direction dir, Point[] result, int resultIndex)
        {
            switch (dir)
            {
                case Direction.LeftToRight:
                    for (int x = xfrom; x < xto; x++)
                    {
                        result[resultIndex] = new Point(x, yfrom);
                    }
                    yfrom++;
                    dir = Direction.TopToBottom;
                    break;
                case Direction.TopToBottom:
                    for (int y = yfrom; y < yto; y++)
                    {
                        result[resultIndex] = new Point(xto - 1, y);
                    }
                    xto--;
                    dir = Direction.RightToLeft;
                    break;
                case Direction.RightToLeft:
                    for (int x = xto - 1; x >= xfrom; x--)
                    {
                        result[resultIndex] = new Point(x, yto - 1);
                    }
                    yto--;
                    dir = Direction.BottomToTop;
                    break;
                case Direction.BottomToTop:
                    for (int y = yto - 1; y >= yfrom; y--)
                    {
                        result[resultIndex] = new Point(xfrom, y);
                    }
                    xfrom++;
                    dir = Direction.LeftToRight;
                    break;
            }
        }
    }
}
