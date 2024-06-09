﻿using System;
using JetBrains.Annotations;

namespace RectpackSharp
{
    /// <summary>
    /// Specifies hints that help optimize the rectangle packing algorithm. 
    /// </summary>
    [Flags]
    [PublicAPI]
    public enum PackingHints
    {
        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by area.</summary>
        TryByArea = 1,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by perimeter.</summary>
        TryByPerimeter = 2,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by bigger side.</summary>
        TryByBiggerSide = 4,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by width.</summary>
        TryByWidth = 8,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by height.</summary>
        TryByHeight = 16,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by a pathological multiplier.</summary>
        TryByPathologicalMultiplier = 32,

        /// <summary>Specifies to try all the possible hints, as to find the best packing configuration.</summary>
        FindBest = TryByArea | TryByPerimeter | TryByBiggerSide | TryByWidth | TryByHeight | TryByPathologicalMultiplier,

        /// <summary>Specifies hints to optimize for rectangles who have one side much bigger than the other.</summary>
        UnusualSizes = TryByPerimeter | TryByBiggerSide | TryByPathologicalMultiplier,

        /// <summary>Specifies hints to optimize for rectangles whose sides are relatively similar.</summary>
        MostlySquared = TryByArea | TryByBiggerSide | TryByWidth | TryByHeight,
    }

    /// <summary>
    /// Provides internal values and functions used by the rectangle packing algorithm.
    /// </summary>
    public static class PackingHintExtensions
    {
        /// <summary>
        /// Represents a method for calculating a sort key from a <see cref="PackingRectangle"/>.
        /// </summary>
        /// <param name="rectangle">The <see cref="PackingRectangle"/> whose sort key to calculate.</param>
        /// <returns>The value that should be assigned to <see cref="PackingRectangle.SortKey"/>.</returns>
        private delegate uint GetSortKeyDelegate(in PackingRectangle rectangle);

        /// <summary>The maximum amount of hints that can be specified by a <see cref="PackingHints"/>.</summary>
        internal const int MaxHintCount = 6;

        private static uint GetArea(in PackingRectangle rectangle) => rectangle.Area;
        private static uint GetPerimeter(in PackingRectangle rectangle) => rectangle.Perimeter;
        private static uint GetBiggerSide(in PackingRectangle rectangle) => rectangle.BiggerSide;
        private static uint GetWidth(in PackingRectangle rectangle) => rectangle.Width;
        private static uint GetHeight(in PackingRectangle rectangle) => rectangle.Height;
        private static uint GetPathologicalMultiplier(in PackingRectangle rectangle) => rectangle.PathologicalMultiplier;

        /// <summary>
        /// Separates a <see cref="PackingHints"/> into the multiple options it contains,
        /// saving each of those separately onto a <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="packingHint">The <see cref="PackingHints"/> to separate.</param>
        /// <param name="span">The span in which to write the resulting hints. This span's excess will be sliced.</param>
        public static void GetFlagsFrom(PackingHints packingHint, ref Span<PackingHints> span)
        {
            int index = 0;
            if (packingHint.HasFlag(PackingHints.TryByArea))
                span[index++] = PackingHints.TryByArea;
            if (packingHint.HasFlag(PackingHints.TryByPerimeter))
                span[index++] = PackingHints.TryByPerimeter;
            if (packingHint.HasFlag(PackingHints.TryByBiggerSide))
                span[index++] = PackingHints.TryByBiggerSide;
            if (packingHint.HasFlag(PackingHints.TryByWidth))
                span[index++] = PackingHints.TryByWidth;
            if (packingHint.HasFlag(PackingHints.TryByHeight))
                span[index++] = PackingHints.TryByHeight;
            if (packingHint.HasFlag(PackingHints.TryByPathologicalMultiplier))
                span[index++] = PackingHints.TryByPathologicalMultiplier;
            
            span = span[..index];
        }

        /// <summary>
        /// Sorts the given <see cref="PackingRectangle"/> array using the specified <see cref="PackingHints"/>.
        /// </summary>
        /// <param name="rectangles">The rectangles to sort.</param>
        /// <param name="packingHint">The hint to sort by. Must be a single bit value.</param>
        /// <remarks>
        /// The <see cref="PackingRectangle.SortKey"/> values will be modified.
        /// </remarks>
        public static Span<PackingRectangle> SortByPackingHint(this Span<PackingRectangle> rectangles, PackingHints packingHint)
        {
            // We first get the appropiate delegate for getting a rectangle's sort key.
            GetSortKeyDelegate getKeyDelegate = packingHint switch
            {
                PackingHints.TryByArea => GetArea,
                PackingHints.TryByPerimeter => GetPerimeter,
                PackingHints.TryByBiggerSide => GetBiggerSide,
                PackingHints.TryByWidth => GetWidth,
                PackingHints.TryByHeight => GetHeight,
                PackingHints.TryByPathologicalMultiplier => GetPathologicalMultiplier,
                _ => throw new ArgumentException(nameof(packingHint))
            };

            // We use the getKeyDelegate to set the sort keys for all the rectangles.
            for (int i = 0; i < rectangles.Length; i++)
                rectangles[i].SortKey = getKeyDelegate(rectangles[i]);

            // We sort the array, using the default rectangle comparison (which compares sort keys).
            rectangles.Sort();
            return rectangles;
        }
    }
}
