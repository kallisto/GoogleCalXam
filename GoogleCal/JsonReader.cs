//
// JsonReader.cs
// Copyright 2011 Alexander Corrado
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Text;
using System.Dynamic;
using System.Collections.Generic;

namespace GEvents
{
    public class JsonReader
    {

        protected TextReader input;
        protected bool eof;
        protected uint row, col;

        public static dynamic Read(Stream json)
        {
            var jso = new JsonReader(new StreamReader(json));
            return jso.Parse();
        }

        public static dynamic Read(string json)
        {
            var jso = new JsonReader(new StringReader(json));
            return jso.Parse();
        }

        protected JsonReader(TextReader json)
        {
            this.input = json;
        }

        protected dynamic Parse()
        {
            var la = Peek();

            /* value:
             *     string
             *     number
             *     object
             *     array
             *     true
             *     false
             *     null
             */

            if (la == '"')
                return ParseString();
            if (char.IsDigit(la) || la == '-')
                return ParseNumber();
            if (la == '{')
                return ParseObject();
            if (la == '[')
                return ParseArray();
            if (la == 't')
            {
                Consume("true");
                return true;
            }
            if (la == 'f')
            {
                Consume("false");
                return false;
            }
            if (la == 'n')
            {
                Consume("null");
                return null;
            }

            Err("unexpected '{0}'", la);
            return null;
        }

        protected string ParseString()
        {
            var next = Peek();
            var sb = new StringBuilder();
            var escaped = false;

            int unicodeShift = -1;
            int unicodeSeq = 0;

            if (next != '"')
                Err("expected string");

            Consume();
            next = Consume();

            while (next != '"' || escaped)
            {

                if (!escaped && next == '\\')
                {

                    escaped = true;

                }
                else if (unicodeShift >= 0)
                {

                    if (char.IsDigit(next))
                        unicodeSeq |= unchecked((int)(next - 48)) << unicodeShift;
                    else if (char.IsLetter(next))
                        unicodeSeq |= unchecked((int)(char.ToUpperInvariant(next) - 65) + 10) << unicodeShift;
                    else
                        Err("malformed Unicode escape sequence");

                    unicodeShift -= 4;
                    if (unicodeShift < 0)
                    {
                        sb.Append((char)unicodeSeq);
                        unicodeSeq = 0;
                    }

                }
                else if (escaped)
                {

                    if (next == 'u')
                    {

                        unicodeShift = 12;

                    }
                    else
                    {

                        switch (next)
                        {

                            case 'a': next = '\a'; break;
                            case 'b': next = '\b'; break;
                            case 'f': next = '\f'; break;
                            case 'n': next = '\n'; break;
                            case 'r': next = '\r'; break;
                            case 't': next = '\t'; break;
                            case 'v': next = '\v'; break;

                        }

                        sb.Append(next);
                    }

                    escaped = false;

                }
                else
                {

                    sb.Append(next);
                }

                next = Consume();
            }

            if (unicodeShift >= 0)
                Err("malformed Unicode escape sequence");

            return sb.ToString();
        }

        protected double ParseNumber()
        {
            /* number:
             *    int
             *    int frac
             *    int exp
             *    int frac exp 
             */

            var next = Peek();
            var sb = new StringBuilder();

            while (char.IsDigit(next) || next == '-' || next == '.' || next == 'e' || next == 'E')
            {
                Consume();
                sb.Append(next);
                next = Peek(true);
            }

            return double.Parse(sb.ToString());
        }

        protected dynamic ParseObject()
        {
            var next = Peek();
            if (next != '{')
                Err("expected object literal");

            Consume();
            IDictionary<string, object> result = new ExpandoObject();

            next = Peek();
            while (next != '}')
            {

                var key = ParseString();
                Consume(':');
                var value = Parse();

                result[key] = value;

                next = Peek();
                if (next != '}')
                    Consume(',');
            }

            Consume('}');
            return result;
        }

        protected IList<dynamic> ParseArray()
        {
            var next = Peek();
            if (next != '[')
                Err("expected array literal");

            Consume();
            var result = new List<dynamic>();

            next = Peek();
            while (next != ']')
            {

                result.Add(Parse());

                next = Peek();
                if (next != ']')
                    Consume(',');
            }

            Consume(']');
            return result;
        }

        // scanner primitives:

        protected void Consume(string expected)
        {
            for (var i = 0; i < expected.Length; i++)
            {

                var actual = Peek(true);
                if (eof || actual != expected[i])
                    Err("expected '{0}'", expected);

                Consume();
            }
        }

        protected void Consume(char expected)
        {
            var actual = Peek();
            if (eof || actual != expected)
                Err("expected '{0}'", expected);

            while (Consume() != actual)
            { /* eat whitespace */ }

        }

        protected char Consume()
        {
            var r = input.Read();
            if (r == -1)
            {
                eof = true;
                return (char)0;
            }

            col++;
            return (char)r;
        }

        protected char Peek()
        {
            return Peek(false);
        }

        protected char Peek(bool whitespaceSignificant)
        {
        top:
            var p = input.Peek();
            if (p == -1)
            {
                eof = true;
                return (char)0;
            }

            var la = (char)p;

            if (!whitespaceSignificant)
            {
                if (la == '\r')
                {
                    input.Read();
                    if (((char)input.Peek()) == '\n')
                        input.Read();

                    col = 1;
                    row++;
                    goto top;
                }

                if (la == '\n')
                {
                    input.Read();
                    col = 1;
                    row++;
                    goto top;
                }

                if (char.IsWhiteSpace(la))
                {
                    Consume();
                    goto top;
                }
            }

            return la;
        }

        protected void Err(string message, params object[] args)
        {
            Consume();
            throw new JsonParseException(row, col - 1, string.Format(message, args));
        }

    }

    public class JsonParseException : Exception
    {
        public JsonParseException(uint row, uint col, string message)
            : base(string.Format("At ({0},{1}): {2}", row, col, message))
        {
        }
    }
}