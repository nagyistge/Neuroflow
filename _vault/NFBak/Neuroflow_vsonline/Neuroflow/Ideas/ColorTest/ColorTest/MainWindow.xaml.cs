using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Devcorp.Controls.Design;

namespace ColorTest
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var splineCalc = new SplineCalc(new[] { new System.Drawing.Point(0, 0), new System.Drawing.Point(128, 64), new System.Drawing.Point(255, 255) });

            var left = Color.FromRgb(1, 70, 121);
            var right = Color.FromRgb(18, 18, 18);

            leftRect.Fill = new SolidColorBrush(left);
            rightRect.Fill = new SolidColorBrush(right);

            var blend = Mix(new[] { new WeightedValue<Color>(left, 1.0), new WeightedValue<Color>(right, 1.0) });

            blendRect.Fill = new SolidColorBrush(blend);
        }

        private static Color Mix(WeightedValue<Color>[] colors)
        {
            double weightSum = colors.Select(vv => vv.Weight).Sum();

            var normalizedHSLColors = colors.Select(vc => new WeightedValue<HSL>(ColorSpaceHelper.RGBtoHSL(vc.Value.R, vc.Value.G, vc.Value.B), vc.Weight / weightSum));

            double h = 0.0;
            double count = 0.0;
            double sSum = 0.0;
            double lSum = 0.0;
            double x = 0.0;
            double y = 0.0;

            foreach (var color in normalizedHSLColors)
            {
                count++;
                sSum += color.Value.Saturation * (color.Weight * 2.0);
                lSum += color.Value.Luminance * (color.Weight * 2.0);
                x += Math.Cos(2.0 * Math.PI * (DegreeToPercent(color.Value.Hue) * (color.Weight * 2.0)));
                y += Math.Sin(2.0 * Math.PI * (DegreeToPercent(color.Value.Hue) * (color.Weight * 2.0)));
            }

            double s = sSum / count;
            double l = lSum / count;

            if ((x != 0.0) || (y != 0.0))
            {
                h = Math.Atan2(y, x) / (2.0 * Math.PI);
            }
            else
            {
                s = 0.0;
            }

            var result = ColorSpaceHelper.HSLtoRGB(PercentToDegree(h), s, l);

            double r = result.Red, g = result.Green, b = result.Blue;

            const double addRatio = 0.0;
            const double avgRatio = 0.0;
            const double subRatio = 0.0;
            const double colorizeRatio = (1.0 - (addRatio + avgRatio + subRatio));

            double additiveR = ComputeAdditiveComponent(colors, weightSum, (c) => c.R);
            double substractiveR = ComputeSubtractiveComponent(colors, weightSum, (c) => c.R);
            double avgR = ComputeAvgComponent(colors, weightSum, (c) => c.R);

            double additiveG = ComputeAdditiveComponent(colors, weightSum, (c) => c.G);
            double substractiveG = ComputeSubtractiveComponent(colors, weightSum, (c) => c.G);
            double avgG = ComputeAvgComponent(colors, weightSum, (c) => c.G);

            double additiveB = ComputeAdditiveComponent(colors, weightSum, (c) => c.B);
            double substractiveB = ComputeSubtractiveComponent(colors, weightSum, (c) => c.B);
            double avgB = ComputeAvgComponent(colors, weightSum, (c) => c.B);

            return Color.FromRgb(
                ToByte(r * colorizeRatio + additiveR * addRatio + substractiveR * subRatio + avgR * avgRatio),
                ToByte(g * colorizeRatio + additiveG * addRatio + substractiveG * subRatio + avgG * avgRatio),
                ToByte(b * colorizeRatio + additiveB * addRatio + substractiveB * subRatio + avgB * avgRatio));
        }

        private static double ComputeAvgComponent(WeightedValue<Color>[] colors, double weightSum, Func<Color, byte> selectComponent)
        {
            return ToByteRange(colors.Select(c => selectComponent(c.Value) * ((c.Weight / weightSum) * 2.0)).Average());
        }

        private static double ComputeSubtractiveComponent(WeightedValue<Color>[] colors, double weightSum, Func<Color, byte> selectComponent)
        {
            return ToByteRange(Flip(Add(colors.Select(c => new WeightedValue<double>(Flip(selectComponent(c.Value)), c.Weight / weightSum)))));
        }

        private static double ComputeAdditiveComponent(WeightedValue<Color>[] colors, double weightSum, Func<Color, byte> selectComponent)
        {
            return ToByteRange(Add(colors.Select(c => new WeightedValue<double>(selectComponent(c.Value), c.Weight / weightSum))));
        }

        private static double DegreeToPercent(double angle)
        {
            return angle / 360.0;
        }

        private static double PercentToDegree(double angle)
        {
            return angle * 360.0;
        }

        private static double Add(IEnumerable<WeightedValue<double>> values)
        {
            //double sum = 0.0;
            //foreach (var vv in values) sum += Math.Pow(vv.Value, 2.0) * (vv.Weight * 2.0);
            //return Math.Sqrt(sum);

            double sum = 0.0;
            foreach (var vv in values) sum += vv.Value * (vv.Weight * 2.0);
            return sum;
        }

        private static double Flip(double value)
        {
            return 255.0 - value;
        }

        private static byte ToByte(double value)
        {
            value = ToByteRange(value);
            return (byte)Math.Round(value);
        }

        private static double ToByteRange(double value)
        {
            if (value < 0.0) value = 0.0; else if (value > 255.0) value = 255.0;
            return value;
        }
    }
}
