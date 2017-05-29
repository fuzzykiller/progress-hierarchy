using System;
using NUnit.Framework;

namespace ConsoleProgressBar.Tests
{
    public class StringExtensionsTests
    {
        [Test]
        public void PadRightSurrogateAware_throws_on_illegal_arguments()
        {
            Assert.That(() => StringExtensions.PadRightSurrogateAware(null, 1), Throws.ArgumentNullException, "'this' == null");
            Assert.That(() => "".PadRightSurrogateAware(-1), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'length' < 0");
            Assert.That(() => "".PadRightSurrogateAware(1, null), Throws.ArgumentNullException, "'ps' == null");
            Assert.That(() => "".PadRightSurrogateAware(1, ""), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'ps' = \"\"");
            Assert.That(() => "".PadRightSurrogateAware(1, "  "), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'ps' == \"  \"");
        }

        [Test]
        public void PadRightSurrogateAware_pads_shorter_strings()
        {
            Assert.That("".PadRightSurrogateAware(5), Is.EqualTo("     "));
            Assert.That("".PadRightSurrogateAware(5, "a"), Is.EqualTo("aaaaa"));
            Assert.That("".PadRightSurrogateAware(5, "🎄"), Is.EqualTo("🎄🎄🎄🎄🎄"));

            Assert.That("a".PadRightSurrogateAware(5), Is.EqualTo("a    "));
            Assert.That("a".PadRightSurrogateAware(5, "b"), Is.EqualTo("abbbb"));
            Assert.That("a".PadRightSurrogateAware(5, "🎄"), Is.EqualTo("a🎄🎄🎄🎄"));

            Assert.That("🎆".PadRightSurrogateAware(5), Is.EqualTo("🎆    "));
            Assert.That("🎆".PadRightSurrogateAware(5, "b"), Is.EqualTo("🎆bbbb"));
            Assert.That("🎆".PadRightSurrogateAware(5, "🎄"), Is.EqualTo("🎆🎄🎄🎄🎄"));

            Assert.That("abc".PadRightSurrogateAware(4), Is.EqualTo("abc "));
            Assert.That("a🎆🎄".PadRightSurrogateAware(4), Is.EqualTo("a🎆🎄 "));
        }

        [Test]
        public void PadRightSurrageAware_leaves_longer_strings_alone()
        {
            Assert.That("Hello World".PadRightSurrogateAware(5), Is.EqualTo("Hello World"));
            Assert.That("Hello".PadRightSurrogateAware(5), Is.EqualTo("Hello"));
        }

        [Test]
        public void SubstringSurrogateAware_throws_on_illegal_arguments()
        {
            Assert.That(() => StringExtensions.SubstringSurrogateAware(null, 1, 1), Throws.ArgumentNullException, "'this' == null");
            Assert.That(() => "".SubstringSurrogateAware(1, 1), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'startIndex' > length");
            Assert.That(() => "".SubstringSurrogateAware(0, 1), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'startIndex' + 'length' > length");
            Assert.That(() => "".SubstringSurrogateAware(0, -1), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'length' < 0");

            Assert.That(() => "a".SubstringSurrogateAware(0, 2), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'startIndex' + 'length' > length");
            Assert.That(() => "a".SubstringSurrogateAware(2, 1), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'startIndex' > length");

            Assert.That(() => "🎆".SubstringSurrogateAware(0, 2), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'startIndex' + 'length' > length");
            Assert.That(() => "🎆".SubstringSurrogateAware(2, 1), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'startIndex' > length");
        }

        [Test]
        public void SubStringSurrogateAware_extracts_correct_substring()
        {
            Assert.That("".SubstringSurrogateAware(0, 0), Is.EqualTo(""));

            Assert.That("a".SubstringSurrogateAware(0, 0), Is.EqualTo(""));
            Assert.That("a".SubstringSurrogateAware(0, 1), Is.EqualTo("a"));
            Assert.That("a".SubstringSurrogateAware(1, 0), Is.EqualTo(""));
            Assert.That("ab".SubstringSurrogateAware(0, 1), Is.EqualTo("a"));
            Assert.That("ab".SubstringSurrogateAware(1, 1), Is.EqualTo("b"));
            Assert.That("ab".SubstringSurrogateAware(0, 2), Is.EqualTo("ab"));

            Assert.That("🎆".SubstringSurrogateAware(0, 0), Is.EqualTo(""));
            Assert.That("🎆".SubstringSurrogateAware(0, 1), Is.EqualTo("🎆"));
            Assert.That("🎆".SubstringSurrogateAware(1, 0), Is.EqualTo(""));
            Assert.That("🎆🎄".SubstringSurrogateAware(0, 1), Is.EqualTo("🎆"));
            Assert.That("🎆🎄".SubstringSurrogateAware(1, 1), Is.EqualTo("🎄"));
            Assert.That("🎆🎄".SubstringSurrogateAware(0, 2), Is.EqualTo("🎆🎄"));
            
            Assert.That("🎆a🎄".SubstringSurrogateAware(0, 1), Is.EqualTo("🎆"));
            Assert.That("🎆a🎄".SubstringSurrogateAware(1, 1), Is.EqualTo("a"));
            Assert.That("🎆a🎄".SubstringSurrogateAware(0, 2), Is.EqualTo("🎆a"));
            Assert.That("🎆a🎄".SubstringSurrogateAware(1, 2), Is.EqualTo("a🎄"));
        }

        [Test]
        public void LimitLength_throws_on_illegal_arguments()
        {
            Assert.That(() => StringExtensions.LimitLength(null, 1), Throws.ArgumentNullException, "'this' == null");
            Assert.That(() => "".LimitLength(-1), Throws.InstanceOf<ArgumentOutOfRangeException>(), "'length'  < 0");
        }

        [Test]
        public void LimitLength_limits_longer_strings_correctly()
        {
            Assert.That("Hello World".LimitLength(6), Is.EqualTo("…World"));
            Assert.That("Hello World".LimitLength(4), Is.EqualTo("orld"));
            Assert.That("Hallo World 🎆".LimitLength(6), Is.EqualTo("…rld 🎆"));
            Assert.That("Hello World".LimitLength(0), Is.EqualTo(""));
            Assert.That("Slightly long-ish version of Hello World 🌟".LimitLength(14), Is.EqualTo("Slig…o World 🌟"));
        }

        [Test]
        public void LimitLength_leaves_shorter_strings_alone()
        {
            Assert.That("Hello World".LimitLength(50), Is.EqualTo("Hello World"));
            Assert.That("Hello World".LimitLength(11), Is.EqualTo("Hello World"));
            Assert.That("Hello".LimitLength(5), Is.EqualTo("Hello"));
            Assert.That("".LimitLength(0), Is.EqualTo(""));
        }
    }
}
