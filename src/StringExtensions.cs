using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ConsoleProgressBar
{
    /// <summary>
    /// Contains extension methods on <see cref="String" />.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Similar to <see cref="string.PadRight(int, char)"/>, but aware of surrogate pairs.
        /// </summary>
        /// <param name="s">String to pad.</param>
        /// <param name="length">Desired length.</param>
        /// <param name="ps">Padding string, defaults to whitespace. Must represent single unicode code point.</param>
        /// <returns>Padded string or original string if no padding was required.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> and/or <paramref name="ps"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ps"/> did not represent single unicode codepoint.</exception>
        public static string PadRightSurrogateAware(this string s, int length, string ps = " ")
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (ps == null) throw new ArgumentNullException(nameof(ps));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (new StringInfo(ps).LengthInTextElements != 1) throw new ArgumentOutOfRangeException(nameof(ps), ps, "Padding string contains no or multiple code points.");

            var currentLength = new StringInfo(s).LengthInTextElements;
            var missingLength = length - currentLength;

            if (missingLength < 1) return s;

            var padding = string.Concat(Enumerable.Repeat(ps, missingLength));
            return s + padding;
        }

        /// <summary>
        /// Similar to <see cref="string.Substring(int, int)"/>, but aware of surrogate pairs.
        /// </summary>
        /// <param name="s">String to extract from.</param>
        /// <param name="startIndex">Start at index.</param>
        /// <param name="length">Number of characters to take.</param>
        /// <returns>Extract from string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="startIndex" /> and <paramref name="length" /> combine such that either end of the extract is out of bounds.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is  <c>null</c>.</exception>
        public static string SubstringSurrogateAware(this string s, int startIndex, int length)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            var stringLength = new StringInfo(s).LengthInTextElements;
            if (startIndex < 0 || startIndex > stringLength)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Start index is not within bounds.");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length is negative.");
            }

            if (startIndex + length > stringLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Start index plus length are not within bounds.");
            }

            var substringTextElements = s.EnumerateTextElements().Skip(startIndex).Take(length);

            return string.Concat(substringTextElements);
        }

        /// <summary>
        /// Limit length of string to given limit, clipping parts as appropriate. Surrogate aware.
        /// </summary>
        /// <param name="s">String to limit in length.</param>
        /// <param name="maxLength">Maximum length.</param>
        /// <returns>Length-limited version of <paramref name="s"/> or <paramref name="s"/> if length was already ≤<paramref name="maxLength"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is smaller than 0.</exception>
        /// <remarks>Not an optimal solution, pending replacement</remarks>
        public static string LimitLength(this string s, int maxLength)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));

            var statusTextLength = new StringInfo(s).LengthInTextElements;
            if (statusTextLength <= maxLength) return s;

            if (maxLength > 12)
            {
                // Lots of space, leave some of the start
                var part2Length = maxLength - 5;
                var part2Start = statusTextLength - part2Length;

                var part1 = s.SubstringSurrogateAware(0, 4) + "…";
                var part2 = s.SubstringSurrogateAware(part2Start, part2Length);
                
                return part1 + part2;
            }

            if (maxLength > 5)
            {
                // Some space, show ellipsis at start
                return "…" + s.SubstringSurrogateAware(statusTextLength - maxLength + 1, maxLength - 1);
            }

            return s.SubstringSurrogateAware(statusTextLength - maxLength, maxLength);
        }

        private static IEnumerable<string> EnumerateTextElements(this string s)
        {
            var enumerator = StringInfo.GetTextElementEnumerator(s);

            while (enumerator.MoveNext())
            {
                yield return enumerator.GetTextElement();
            }
        }
    }
}
