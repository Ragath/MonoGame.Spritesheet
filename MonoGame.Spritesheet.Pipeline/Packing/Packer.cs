using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MonoGame.Spritesheet.Pipeline.Packing
{
    static class Packer
    {
        public static int GetArea(this Rectangle rect) => rect.Width * rect.Height;
        public static int GetArea(this IEnumerable<Rectangle> rects) => rects.Sum(r => r.GetArea());
        public static int GetArea(this IEnumerable<(int, Rectangle)> rects) => rects.Sum(r => r.Item2.GetArea());
        static Rectangle GetUnion(this IEnumerable<Rectangle> rects) => rects.Aggregate((total, r) => Rectangle.Union(r, total));

        public static int GetUnionArea(this IEnumerable<Rectangle> rects) => (rects.Max(r => r.Right) - rects.Min(r => r.Left)) * (rects.Max(r => r.Bottom) - rects.Min(r => r.Top));

        public static Rectangle[] Pack(Rectangle[] input, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
        {
            var rects = (from r in input.Select((rect, id) => (id: id, rect: rect))
                         orderby Math.Max(r.rect.Width, r.rect.Height) //* 1 + r.GetArea() * 0
                         select r).ToArray();

            var size = GetStartingSize(rects);
            if (size.h * size.w * 1.5f > rects.GetArea())
                size.w >>= 1;
            size.w = Math.Min(size.w, maxWidth);
            size.h = maxHeight;

            var bin = new MaxRectsBinPack(size.w, size.h, canFlip: false);
            bin.Insert(rects, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBottomLeftRule);

            var output = bin.usedRectangles.OrderBy(r => r.id).Select(r => r.rect).ToArray();
            var minBinArea = output.GetUnionArea();
            if (rects.Length != bin.usedRectangles.Count)
                throw new Exception($"{nameof(bin.usedRectangles)}: {bin.usedRectangles.Count} Expected: {rects.Length}");

            var inputUnionArea = input.GetUnionArea();
            if (inputUnionArea < minBinArea && inputUnionArea > input.GetArea())
                return input.ToArray();
            else
                return output;
        }

        static (int w, int h) GetStartingSize((int, Rectangle)[] rects)
        {
            var s = Math.Sqrt(rects.Sum(r => r.Item2.GetArea()));
            var mw = rects.Max(r => r.Item2.Width);

            var w = 1;
            while (w < mw || w < s)
                w <<= 1;

            var h = 1;
            while (h < s)
                h <<= 1;

            return (w, h);
        }
    }
}
