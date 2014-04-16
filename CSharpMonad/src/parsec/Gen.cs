﻿////////////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
// 
// Copyright (c) 2014 Paul Louth
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.Parsec
{
    /// <summary>
    /// General parsers
    /// TODO: Comments
    /// </summary>
    public static partial class Gen
    {
        public static Item Item()
        {
            return new Item();
        }
        public static Empty<T> Empty<T>()
        {
            return new Empty<T>();
        }
        public static Failure<T> Failure<T>(ParserError error)
        {
            return new Failure<T>(error);
        }
        public static Failure<T> Failure<T>(ParserError error, IEnumerable<ParserError> errors)
        {
            return new Failure<T>(error, errors);
        }
        public static Return<T> Return<T>(T v)
        {
            return new Return<T>(v);
        }
        public static Choice<A> Choice<A>(Parser<A> p, params Parser<A>[] ps)
        {
            return new Choice<A>(p, ps);
        }
        public static Choice<A> Choice<A>(Parser<A> p, IEnumerable<Parser<A>> ps)
        {
            return new Choice<A>(p, ps);
        }
        public static Choice<A> Choice<A>(IEnumerable<Parser<A>> ps)
        {
            return new Choice<A>(ps);
        }
        public static Satisfy Satisfy(Func<char, bool> predicate, string expecting = "")
        {
            return new Satisfy(predicate, expecting);
        }
        public static OneOf OneOf(string chars)
        {
            return new OneOf(chars);
        }
        public static OneOf OneOf(IEnumerable<char> chars)
        {
            return new OneOf(chars);
        }
        public static OneOf OneOf(IEnumerable<ParserChar> chars)
        {
            return new OneOf(chars);
        }
        public static NoneOf NoneOf(string chars)
        {
            return new NoneOf(chars);
        }
        public static NoneOf NoneOf(IEnumerable<char> chars)
        {
            return new NoneOf(chars);
        }
        public static NoneOf NoneOf(IEnumerable<ParserChar> chars)
        {
            return new NoneOf(chars);
        }
        public static Parser<A> SepBy<A,B>(Parser<A> parser, Parser<B> sepParser)
        {
            return SepBy1<A, B>(parser, sepParser) | Gen.Empty<A>();
        }
        public static Parser<A> SepBy1<A, B>(Parser<A> parser, Parser<B> sepParser)
        {
            return new Parser<A>( inp =>
            {
                var x = parser.Parse(inp);
                if (x.IsFaulted)
                    return x;

                var xs = Gen.Many<A>(sepParser.And(parser)).Parse(x.Value.Last().Item2);
                if (x.IsFaulted)
                    return xs;

                return new ParserResult<A>(x.Value.Concat(xs.Value));
            });
        }

        public static Digit Digit()
        {
            return new Digit();
        }
        public static HexDigit HexDigit()
        {
            return new HexDigit();
        }
        public static OctalDigit OctalDigit()
        {
            return new OctalDigit();
        }
        public static Letter Letter()
        {
            return new Letter();
        }
        public static LetterOrDigit LetterOrDigit()
        {
            return new LetterOrDigit();
        }
        public static Integer Integer()
        {
            return new Integer();
        }
        public static Character Character(char c)
        {
            return new Character(c);
        }
        public static Many<T> Many<T>(Parser<T> parser)
        {
            return new Many<T>(parser);
        }
        public static Many1<T> Many1<T>(Parser<T> parser)
        {
            return new Many1<T>(parser);
        }
        public static Try<T> Try<T>(Parser<T> parser)
        {
            return new Try<T>(parser);
        }
        public static StringParse String(string str)
        {
            return new StringParse(str);
        }
        public static StringParse String(IEnumerable<char> str)
        {
            return new StringParse(str);
        }
        public static ParserChar ParserChar(char c, SrcLoc location = null)
        {
            return new ParserChar(c, location);
        }
        public static WhiteSpace WhiteSpace()
        {
            return new WhiteSpace();
        }
        public static SimpleSpace SimpleSpace()
        {
            return new SimpleSpace();
        }
        public static NotFollowedBy<A> NotFollowedBy<A>(Parser<A> followParser)
        {
            return new NotFollowedBy<A>(followParser);
        }

        public static Between<O, C, B> Between<O, C, B>(Parser<O> openParser, Parser<C> closeParser, Parser<B> betweenParser)
        {
            return new Between<O, C, B>(openParser,closeParser,betweenParser);
        }

        public static Parser<Unit> SkipMany1<A>(Parser<A> skipParser)
        {
            return new Parser<Unit>(
                inp =>
                {
                    var resA = skipParser.Parse(inp);
                    return resA.IsFaulted
                        ? new ParserResult<Unit>(resA.Errors)
                        : SkipMany<A>(skipParser).Parse(inp);
                }
            );
        }

        public static Parser<Unit> SkipMany<A>(Parser<A> skipParser)
        {
            return new Parser<Unit>(
                inp =>
                {
                    if( inp.IsEmpty() ) 
                        return Gen.Return<Unit>(Unit.Return()).Parse(inp);

                    do
                    {
                        var head = inp.Head();

                        var resA = skipParser.Parse(inp);
                        if (resA.IsFaulted || resA.Value.IsEmpty())
                            return Gen.Return<Unit>(Unit.Return()).Parse(inp);

                        inp = resA.Value.Last().Item2;
                    }
                    while (!inp.IsEmpty());

                    return Gen.Return<Unit>(Unit.Return()).Parse(inp);
                }
            );
        }

        public static Func<A, Parser<ParserChar>> FromDelegate<A>(Func<A,Func<IEnumerable<ParserChar>, ParserResult<ParserChar>>> func)
        {
            return FromDelegate<A, ParserChar>(func);
        }

        public static Func<A, Parser<P>> FromDelegate<A, P>(Func<A, Func<IEnumerable<ParserChar>, ParserResult<P>>> func)
        {
            return (A a) => new Parser<P>(func(a));
        }
    }
}