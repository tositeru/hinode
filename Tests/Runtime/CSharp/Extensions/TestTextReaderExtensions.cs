using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    public class TestTextReaderExtensions
    {
        [Test]
        public void MoveToPasses()
        {
            string text = "apple orange grape";
            using (var reader = new StringReader(text))
            {
                Assert.IsTrue(reader.MoveTo(' '));
                Assert.AreEqual('o', (char)reader.Peek());

                Assert.IsTrue(reader.MoveTo(' '));
                Assert.AreEqual('g', (char)reader.Peek());

                Assert.IsFalse(reader.MoveTo(' '));
                Assert.AreEqual(-1, reader.Read());
            }

            using (var reader = new StringReader("Apple Orange Grape"))
            {
                var skipKeyChar = new char[] { 'e', 'g' };
                Assert.IsTrue(reader.MoveTo(skipKeyChar));
                Assert.AreEqual(' ', (char)reader.Peek());

                Assert.IsTrue(reader.MoveTo(skipKeyChar));
                Assert.AreEqual('e', (char)reader.Peek());

                Assert.IsTrue(reader.MoveTo(skipKeyChar));
                Assert.AreEqual(' ', (char)reader.Peek());

                Assert.IsTrue(reader.MoveTo(skipKeyChar));
                Assert.AreEqual(-1, reader.Peek());

                Assert.IsFalse(reader.MoveTo(skipKeyChar));
                Assert.AreEqual(-1, reader.Peek());
            }

            using (var reader = new StringReader("Apple Orange Grape"))
            {
                var regex = new Regex(@"[eg]", RegexOptions.IgnoreCase);
                Assert.IsTrue(reader.MeveTo(regex));
                Assert.AreEqual(' ', (char)reader.Peek());

                Assert.IsTrue(reader.MeveTo(regex));
                Assert.AreEqual('e', (char)reader.Peek());

                Assert.IsTrue(reader.MeveTo(regex));
                Assert.AreEqual(' ', (char)reader.Peek());

                Assert.IsTrue(reader.MeveTo(regex));
                Assert.AreEqual('r', (char)reader.Peek());

                Assert.IsTrue(reader.MeveTo(regex));
                Assert.AreEqual(-1, reader.Peek());

                Assert.IsFalse(reader.MeveTo(regex));
                Assert.AreEqual(-1, reader.Peek());
            }
        }

        [Test]
        public void SkipToPasses()
        {
            string text = "aaa aaBCaaaaDE";
            using (var reader = new StringReader(text))
            {
                Assert.IsTrue(reader.SkipTo('a'));
                Assert.AreEqual(' ', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo('a'));
                Assert.AreEqual('B', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo('a'));
                Assert.AreEqual('C', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo('a'));
                Assert.AreEqual('D', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo('a'));
                Assert.AreEqual('E', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsFalse(reader.SkipTo('a'));
                Assert.AreEqual(-1, reader.Peek());
            }

            using (var reader = new StringReader("1122A222BC11D"))
            {
                var skipKeyChar = new char[] { '1', '2' };
                Assert.IsTrue(reader.SkipTo(skipKeyChar));
                Assert.AreEqual('A', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(skipKeyChar));
                Assert.AreEqual('B', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(skipKeyChar));
                Assert.AreEqual('C', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(skipKeyChar));
                Assert.AreEqual('D', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsFalse(reader.SkipTo(skipKeyChar));
                Assert.AreEqual(-1, reader.Peek());
            }

            using (var reader = new StringReader("1aaa23AaA4"))
            {
                var regex = new Regex(@"[a]", RegexOptions.IgnoreCase);
                Assert.IsTrue(reader.SkipTo(regex));
                Assert.AreEqual('1', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(regex));
                Assert.AreEqual('2', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(regex));
                Assert.AreEqual('3', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(regex));
                Assert.AreEqual('4', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsFalse(reader.SkipTo(regex));
                Assert.AreEqual(-1, reader.Peek());
            }
        }

        [Test]
        public void IsMatchPeekPasses()
        {
            using (var reader = new StringReader("ab1ABC"))
            {
                Assert.IsTrue(reader.IsMatchPeek('a'));

                reader.Read(); // move to next
                Assert.IsTrue(reader.IsMatchPeek('b', '1'));
                reader.Read(); // move to next
                Assert.IsTrue(reader.IsMatchPeek('b', '1'));

                reader.Read(); // move to next
                Assert.IsTrue(reader.IsMatchPeek(new Regex(@"a", RegexOptions.IgnoreCase)));
            }

            using (var reader = new StringReader("1122A222BC11D"))
            {
                var skipKeyChar = new char[] { '1', '2' };
                Assert.IsTrue(reader.SkipTo(skipKeyChar));
                Assert.AreEqual('A', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(skipKeyChar));
                Assert.AreEqual('B', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(skipKeyChar));
                Assert.AreEqual('C', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(skipKeyChar));
                Assert.AreEqual('D', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsFalse(reader.SkipTo(skipKeyChar));
                Assert.AreEqual(-1, reader.Peek());
            }

            using (var reader = new StringReader("1aaa23AaA4"))
            {
                var regex = new Regex(@"[a]", RegexOptions.IgnoreCase);
                Assert.IsTrue(reader.SkipTo(regex));
                Assert.AreEqual('1', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(regex));
                Assert.AreEqual('2', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(regex));
                Assert.AreEqual('3', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsTrue(reader.SkipTo(regex));
                Assert.AreEqual('4', (char)reader.Peek());

                reader.Read(); // move to next
                Assert.IsFalse(reader.SkipTo(regex));
                Assert.AreEqual(-1, reader.Peek());
            }
        }
    }
}
