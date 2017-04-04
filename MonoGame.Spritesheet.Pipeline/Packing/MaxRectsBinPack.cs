using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MonoGame.Spritesheet.Pipeline.Packing
{
    public class MaxRectsBinPack
    {
        /// <summary>
        /// Specifies the different heuristic rules that can be used when deciding where to place a new rectangle.
        /// </summary>
        public enum FreeRectChoiceHeuristic
        {
            RectBestShortSideFit, ///< -BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
            RectBestLongSideFit, ///< -BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
            RectBestAreaFit, ///< -BAF: Positions the rectangle into the smallest free rect into which it fits.
            RectBottomLeftRule, ///< -BL: Does the Tetris placement.
            RectContactPointRule ///< -CP: Choosest the placement where the rectangle touches other rects as much as possible.
        };

        public int binWidth;
        public int binHeight;

        public List<(int id, Rectangle rect)> usedRectangles { get; } = new List<(int id, Rectangle rect)>();
        public bool CanFlip { get; }

        List<Rectangle> freeRectangles = new List<Rectangle>();

        public MaxRectsBinPack(int width, int height, bool canFlip = false)
        {
            binWidth = width;
            binHeight = height;
            CanFlip = canFlip;

            var n = new Rectangle
            {
                X = 0,
                Y = 0,
                Width = width,
                Height = height
            };

            usedRectangles.Clear();

            freeRectangles.Clear();
            freeRectangles.Add(n);
        }

        Rectangle Insert(int id, int width, int height, FreeRectChoiceHeuristic method)
        {
            Rectangle newNode;
            // Unused in this function. We don't need to know the score after finding the position.
            int score1 = int.MaxValue;
            int score2 = int.MaxValue;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit:
                    newNode = FindPositionForNewNodeBestShortSideFit(width, height, out score1, out score2);
                    break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule:
                    newNode = FindPositionForNewNodeBottomLeft(width, height, out score1, out score2);
                    break;
                case FreeRectChoiceHeuristic.RectContactPointRule:
                    newNode = FindPositionForNewNodeContactPoint(width, height, out score1);
                    break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit:
                    newNode = FindPositionForNewNodeBestLongSideFit(width, height, out score2, out score1);
                    break;
                case FreeRectChoiceHeuristic.RectBestAreaFit:
                    newNode = FindPositionForNewNodeBestAreaFit(width, height, out score1, out score2);
                    break;
                default:
                    throw new ArgumentException(nameof(method));
            }

            if (newNode.Height == 0)
                return newNode;

            var count = freeRectangles.Count;
            for (var i = 0; i < count; i++)
                if (SplitFreeNode(freeRectangles[i], ref newNode))
                {
                    freeRectangles.RemoveAt(i);
                    i--;
                    count--;
                }

            PruneFreeList();

            usedRectangles.Add((id, newNode));
            return newNode;
        }

        public void Insert(IEnumerable<(int id, Rectangle rect)> input, FreeRectChoiceHeuristic method)
        {
            var rects = input.ToList();

            while (rects.Count > 0)
            {
                int bestScore1 = int.MaxValue;
                int bestScore2 = int.MaxValue;
                int bestRectIndex = -1;
                Rectangle bestNode = Rectangle.Empty;
                int bestRectId = -1;

                for (var i = 0; i < rects.Count; i++)
                {
                    var rect = rects[i].rect;
                    var newNode = ScoreRect(rect.Width, rect.Height, method, out var score1, out var score2);

                    if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2))
                    {
                        bestScore1 = score1;
                        bestScore2 = score2;
                        bestNode = newNode;
                        bestRectIndex = i;
                        bestRectId = rects[i].id;
                    }
                }

                if (bestRectIndex == -1)
                    return;

                PlaceRect(bestRectId, ref bestNode);
                rects.RemoveAt(bestRectIndex);
            }
        }

        void PlaceRect(int id, ref Rectangle node)
        {
            var count = freeRectangles.Count;
            for (var i = 0; i < count; i++)
                if (SplitFreeNode(freeRectangles[i], ref node))
                {
                    freeRectangles.RemoveAt(i);
                    i--;
                    count--;
                }

            PruneFreeList();

            usedRectangles.Add((id, node));
        }

        Rectangle ScoreRect(int width, int height, FreeRectChoiceHeuristic method, out int score1, out int score2)
        {
            Rectangle newNode;
            score1 = int.MaxValue;
            score2 = int.MaxValue;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit:
                    newNode = FindPositionForNewNodeBestShortSideFit(width, height, out score1, out score2); break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule:
                    newNode = FindPositionForNewNodeBottomLeft(width, height, out score1, out score2); break;
                case FreeRectChoiceHeuristic.RectContactPointRule:
                    newNode = FindPositionForNewNodeContactPoint(width, height, out score1);
                    score1 = -score1; // Reverse since we are minimizing, but for contact point score bigger is better.
                    break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit:
                    newNode = FindPositionForNewNodeBestLongSideFit(width, height, out score2, out score1);
                    break;
                case FreeRectChoiceHeuristic.RectBestAreaFit:
                    newNode = FindPositionForNewNodeBestAreaFit(width, height, out score1, out score2);
                    break;
                default:
                    throw new ArgumentException(nameof(method));
            }

            // Cannot fit the current rectangle.
            if (newNode.Height == 0)
            {
                score1 = int.MaxValue;
                score2 = int.MaxValue;
            }

            return newNode;
        }

        Rectangle FindPositionForNewNodeBottomLeft(int width, int height, out int bestY, out int bestX)
        {

            Rectangle bestNode = Rectangle.Empty;

            bestY = int.MaxValue;
            bestX = int.MaxValue;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    int topSideY = freeRectangles[i].Y + height;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].X < bestX))
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestY = topSideY;
                        bestX = freeRectangles[i].X;
                    }
                }
                if (CanFlip && freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                    int topSideY = freeRectangles[i].Y + width;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].X < bestX))
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestY = topSideY;
                        bestX = freeRectangles[i].X;
                    }
                }
            }
            return bestNode;
        }

        Rectangle FindPositionForNewNodeBestShortSideFit(int width, int height, out int bestShortSideFit, out int bestLongSideFit)
        {
            Rectangle bestNode = Rectangle.Empty;

            bestShortSideFit = int.MaxValue;
            bestLongSideFit = int.MaxValue;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].Width - width);
                    int leftoverVert = Math.Abs(freeRectangles[i].Height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (shortSideFit < bestShortSideFit || (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (CanFlip && freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                    int flippedLeftoverHoriz = Math.Abs(freeRectangles[i].Width - height);
                    int flippedLeftoverVert = Math.Abs(freeRectangles[i].Height - width);
                    int flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                    int flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                    if (flippedShortSideFit < bestShortSideFit || (flippedShortSideFit == bestShortSideFit && flippedLongSideFit < bestLongSideFit))
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestShortSideFit = flippedShortSideFit;
                        bestLongSideFit = flippedLongSideFit;
                    }
                }
            }
            return bestNode;
        }

        Rectangle FindPositionForNewNodeBestLongSideFit(int width, int height, out int bestShortSideFit, out int bestLongSideFit)
        {

            Rectangle bestNode = Rectangle.Empty;

            bestShortSideFit = int.MaxValue;
            bestLongSideFit = int.MaxValue;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].Width - width);
                    int leftoverVert = Math.Abs(freeRectangles[i].Height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (CanFlip && freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].Width - height);
                    int leftoverVert = Math.Abs(freeRectangles[i].Height - width);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }
            }
            return bestNode;
        }

        Rectangle FindPositionForNewNodeBestAreaFit(int width, int height, out int bestAreaFit, out int bestShortSideFit)
        {

            Rectangle bestNode = Rectangle.Empty;

            bestAreaFit = int.MaxValue;
            bestShortSideFit = int.MaxValue;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                int areaFit = freeRectangles[i].Width * freeRectangles[i].Height - width * height;

                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].Width - width);
                    int leftoverVert = Math.Abs(freeRectangles[i].Height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }

                if (CanFlip && freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].Width - height);
                    int leftoverVert = Math.Abs(freeRectangles[i].Height - width);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }
            }
            return bestNode;
        }

        /// <summary>
        /// Get overlap length
        /// </summary>
        /// <param name="i1start"></param>
        /// <param name="i1end"></param>
        /// <param name="i2start"></param>
        /// <param name="i2end"></param>
        /// <returns>0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.</returns>
        int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
        {
            if (i1end < i2start || i2end < i1start)
                return 0;
            return Math.Min(i1end, i2end) - Math.Max(i1start, i2start);
        }

        int ContactPointScoreNode(int x, int y, int width, int height)
        {
            int score = 0;

            if (x == 0 || x + width == binWidth)
                score += height;
            if (y == 0 || y + height == binHeight)
                score += width;

            for (var i = 0; i < usedRectangles.Count; ++i)
            {
                var rect = usedRectangles[i].rect;
                if (rect.X == x + width || rect.X + rect.Width == x)
                    score += CommonIntervalLength(rect.Y, rect.Y + rect.Height, y, y + height);
                if (rect.Y == y + height || rect.Y + rect.Height == y)
                    score += CommonIntervalLength(rect.X, rect.X + rect.Width, x, x + width);
            }
            return score;
        }

        Rectangle FindPositionForNewNodeContactPoint(int width, int height, out int bestContactScore)
        {
            Rectangle bestNode = Rectangle.Empty;

            bestContactScore = -1;

            for (var i = 0; i < freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (freeRectangles[i].Width >= width && freeRectangles[i].Height >= height)
                {
                    int score = ContactPointScoreNode(freeRectangles[i].X, freeRectangles[i].Y, width, height);
                    if (score > bestContactScore)
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestContactScore = score;
                    }
                }

                if (CanFlip && freeRectangles[i].Width >= height && freeRectangles[i].Height >= width)
                {
                    int score = ContactPointScoreNode(freeRectangles[i].X, freeRectangles[i].Y, height, width);
                    if (score > bestContactScore)
                    {
                        bestNode.X = freeRectangles[i].X;
                        bestNode.Y = freeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestContactScore = score;
                    }
                }
            }
            return bestNode;
        }

        bool SplitFreeNode(Rectangle freeNode, ref Rectangle usedNode)
        {
            // Test with SAT if the rectangles even intersect.
            if (usedNode.X >= freeNode.X + freeNode.Width || usedNode.X + usedNode.Width <= freeNode.X ||
                usedNode.Y >= freeNode.Y + freeNode.Height || usedNode.Y + usedNode.Height <= freeNode.Y)
                return false;

            if (usedNode.X < freeNode.X + freeNode.Width && usedNode.X + usedNode.Width > freeNode.X)
            {
                // New node at the top side of the used node.
                if (usedNode.Y > freeNode.Y && usedNode.Y < freeNode.Y + freeNode.Height)
                {
                    Rectangle newNode = freeNode;
                    newNode.Height = usedNode.Y - newNode.Y;
                    freeRectangles.Add(newNode);
                }

                // New node at the bottom side of the used node.
                if (usedNode.Y + usedNode.Height < freeNode.Y + freeNode.Height)
                {
                    Rectangle newNode = freeNode;
                    newNode.Y = usedNode.Y + usedNode.Height;
                    newNode.Height = freeNode.Y + freeNode.Height - (usedNode.Y + usedNode.Height);
                    freeRectangles.Add(newNode);
                }
            }

            if (usedNode.Y < freeNode.Y + freeNode.Height && usedNode.Y + usedNode.Height > freeNode.Y)
            {
                // New node at the left side of the used node.
                if (usedNode.X > freeNode.X && usedNode.X < freeNode.X + freeNode.Width)
                {
                    Rectangle newNode = freeNode;
                    newNode.Width = usedNode.X - newNode.X;
                    freeRectangles.Add(newNode);
                }

                // New node at the right side of the used node.
                if (usedNode.X + usedNode.Width < freeNode.X + freeNode.Width)
                {
                    Rectangle newNode = freeNode;
                    newNode.X = usedNode.X + usedNode.Width;
                    newNode.Width = freeNode.X + freeNode.Width - (usedNode.X + usedNode.Width);
                    freeRectangles.Add(newNode);
                }
            }

            return true;
        }

        void PruneFreeList()
        {
            /* 
            ///  Would be nice to do something like this, to avoid a Theta(n^2) loop through each pair.
            ///  But unfortunately it doesn't quite cut it, since we also want to detect containment. 
            ///  Perhaps there's another way to do this faster than Theta(n^2).

            if (freeRectangles.size() > 0)
                clb::sort::QuickSort(&freeRectangles[0], freeRectangles.size(), NodeSortCmp);

            for(size_t i = 0; i < freeRectangles.size()-1; ++i)
                if (freeRectangles[i].x == freeRectangles[i+1].x &&
                    freeRectangles[i].y == freeRectangles[i+1].y &&
                    freeRectangles[i].width == freeRectangles[i+1].width &&
                    freeRectangles[i].height == freeRectangles[i+1].height)
                {
                    freeRectangles.erase(freeRectangles.begin() + i);
                    --i;
                }
            */

            /// Go through each pair and remove any rectangle that is redundant.
            for (var i = 0; i < freeRectangles.Count; ++i)
                for (var j = i + 1; j < freeRectangles.Count; ++j)
                {
                    if (IsContainedIn(freeRectangles[i], freeRectangles[j]))
                    {
                        freeRectangles.RemoveAt(i);
                        --i;
                        break;
                    }
                    if (IsContainedIn(freeRectangles[j], freeRectangles[i]))
                    {
                        freeRectangles.RemoveAt(j);
                        --j;
                    }
                }
        }

        static bool IsContainedIn(Rectangle a, Rectangle b)
            => a.X >= b.X && a.Y >= b.Y
            && a.X + a.Width <= b.X + b.Width
            && a.Y + a.Height <= b.Y + b.Height;
    }
}
