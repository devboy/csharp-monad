////////////////////////////////////////////////////////////////////////////////////////
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

using Monad.Utility;
using System;
using System.Collections.Generic;

namespace Monad
{
    /// <summary>
    /// Either monad
    /// </summary>
    public delegate EitherPair<R, L> Either<R, L>();

    public struct EitherPair<R, L>
    {
        public readonly R Right;
        public readonly L Left;
        public readonly bool IsRight;
        public readonly bool IsLeft;

        public EitherPair(R r)
        {
            Right = r;
            Left = default(L);
            IsRight = true;
            IsLeft = false;
        }

        public EitherPair(L l)
        {
            Left = l;
            Right = default(R);
            IsLeft = true;
            IsRight = false;
        }

        public static implicit operator EitherPair<R, L>(L value)
        {
            return new EitherPair<R, L>(value);
        }

        public static implicit operator EitherPair<R, L>(R value)
        {
            return new EitherPair<R, L>(value);
        }
    }

    /// <summary>
    /// Either constructor methods
    /// </summary>
    public class Either
    {
        /// <summary>
        /// Construct an Either Left monad
        /// </summary>
        public static Either<R, L> Left<R, L>(Func<L> left)
        {
            return () => new EitherPair<R, L>(left());
        }

        /// <summary>
        /// Construct an Either Right monad
        /// </summary>
        public static Either<R, L> Right<R, L>(Func<R> right)
        {
            return () => new EitherPair<R, L>(right());
        }

        /// <summary>
        /// Construct an either Left or Right
        /// </summary>
        public static Either<R, L> Return<R, L>(Func<EitherPair<R, L>> either)
        {
            return () => either();
        }

        /// <summary>
        /// Monadic zero
        /// </summary>
        public static Either<R, L> Mempty<R, L>()
        {
            return () => new EitherPair<R, L>(default(R));
        }
    }

    /// <summary>
    /// The Either monad represents values with two possibilities: a value of Left or Right
    /// Either is sometimes used to represent a value which is either correct or an error, 
    /// by convention, 'Left' is used to hold an error value 'Right' is used to hold a 
    /// correct value.
    /// So you can see that Either has a very close relationship to the Error monad.  However,
    /// the Either monad won't capture exceptions.  Either would primarily be used for 
    /// known error values rather than exceptional ones.
    /// Once the Either monad is in the Left state it cancels the monad bind function and 
    /// returns immediately.
    /// </summary>
    public static class EitherExt
    {
        /// <summary>
        /// Returns true if the monad object is in the Right state
        /// </summary>
        public static bool IsRight<R, L>(this Either<R, L> m)
        {
            return m().IsRight;
        }

        /// <summary>
        /// Get the Left value
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Right state
        /// </summary>
        public static bool IsLeft<R, L>(this Either<R, L> m)
        {
            return m().IsLeft;
        }

        /// <summary>
        /// Get the Right value
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Left state
        /// </summary>
        public static R Right<R, L>(this Either<R, L> m)
        {
            var res = m();
            if (res.IsLeft)
                throw new InvalidOperationException("Not in the Right state");
            return res.Right;
        }

        /// <summary>
        /// Get the Left value
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Right state
        /// </summary>
        public static L Left<R, L>(this Either<R, L> m)
        {
            var res = m();
            if (res.IsRight)
                throw new InvalidOperationException("Not in the Left state");
            return res.Left;
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// </summary>
        /// <param name="Right">Action to perform if the monad is in the Right state</param>
        /// <param name="Left">Action to perform if the monad is in the Left state</param>
        /// <returns>T</returns>
        public static Func<T> Match<R, L, T>(this Either<R, L> m, Func<R, T> Right, Func<L, T> Left)
        {
            return () =>
            {
                var res = m();
                return res.IsLeft
                    ? Left(res.Left)
                    : Right(res.Right);
            };
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Left state
        /// </summary>
        /// <param name="right">Action to perform if the monad is in the Right state</param>
        /// <returns>T</returns>
        public static Func<T> MatchRight<R, L, T>(this Either<R, L> m, Func<R, T> right)
        {
            return () =>
            {
                return right(m.Right());
            };
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Right state
        /// </summary>
        /// <param name="left">Action to perform if the monad is in the Left state</param>
        /// <returns>T</returns>
        public static Func<T> MatchLeft<R, L, T>(this Either<R, L> m, Func<L, T> left)
        {
            return () =>
            {
                return left(m.Left());
            };
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// Returns the defaultValue if the monad is in the Left state
        /// </summary>
        /// <param name="right">Action to perform if the monad is in the Right state</param>
        /// <returns>T</returns>
        public static Func<T> MatchRight<R, L, T>(this Either<R, L> m, Func<R, T> right, T defaultValue)
        {
            return () =>
            {
                var res = m();
                if (res.IsLeft)
                    return defaultValue;
                return right(res.Right);
            };
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// Returns the defaultValue if the monad is in the Right state
        /// </summary>
        /// <param name="left">Action to perform if the monad is in the Left state</param>
        /// <returns>T</returns>
        public static Func<T> MatchLeft<R, L, T>(this Either<R, L> m, Func<L, T> left, T defaultValue)
        {
            return () =>
            {
                var res = m();
                if (res.IsRight)
                    return defaultValue;
                return left(res.Left);
            };
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// </summary>
        /// <param name="Right">Action to perform if the monad is in the Right state</param>
        /// <param name="Left">Action to perform if the monad is in the Left state</param>
        /// <returns>Unit</returns>
        public static Func<Unit> Match<R, L>(this Either<R, L> m, Action<R> Right, Action<L> Left)
        {
            return () =>
            {
                var res = m();
                if (res.IsLeft)
                    Left(res.Left);
                else
                    Right(res.Right);
                return Unit.Default;
            };
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Left state
        /// </summary>
        /// <param name="right">Action to perform if the monad is in the Right state</param>
        /// <returns>Unit</returns>
        public static Func<Unit> MatchRight<R, L>(this Either<R, L> m, Action<R> right)
        {
            return () =>
            {
                right(m.Right());
                return Unit.Default;
            };
        }

        /// <summary>
        /// Pattern matching method for a branching expression
        /// NOTE: This throws an InvalidOperationException if the object is in the 
        /// Right state
        /// </summary>
        /// <param name="left">Action to perform if the monad is in the Left state</param>
        /// <returns>Unit</returns>
        public static Func<Unit> MatchLeft<R, L>(this Either<R, L> m, Action<L> left)
        {
            return () =>
            {
                left(m.Left());
                return Unit.Default;
            };
        }

        /// <summary>
        /// Monadic append
        /// If the left-hand side or right-hand side are in a Left state, then Left propagates
        /// </summary>
        public static Either<R, L> Mappend<R, L>(this Either<R, L> lhs, Either<R, L> rhs)
        {
            return () =>
            {
                var lhsV = lhs();
                if (lhsV.IsLeft)
                {
                    return lhsV;
                }
                else
                {
                    var rhsV = rhs();
                    if (rhsV.IsLeft)
                    {
                        return rhsV;
                    }
                    else
                    {
                        if (lhsV.Right is IAppendable<R>)
                        {
                            var lhsApp = lhsV.Right as IAppendable<R>;
                            return new EitherPair<R, L>(lhsApp.Append(rhsV.Right));
                        }
                        else
                        {
                            // TODO: Consider replacing this with a static Reflection.Emit which does this job efficiently.
                            switch (typeof(R).ToString())
                            {
                                case "System.Int64":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToInt64(lhsV.Right) + Convert.ToInt64(rhsV.Right)), typeof(R)));
                                case "System.UInt64":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToUInt64(lhsV.Right) + Convert.ToUInt64(rhsV.Right)), typeof(R)));
                                case "System.Int32":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToInt32(lhsV.Right) + Convert.ToInt32(rhsV.Right)), typeof(R)));
                                case "System.UInt32":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToUInt32(lhsV.Right) + Convert.ToUInt32(rhsV.Right)), typeof(R)));
                                case "System.Int16":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToInt16(lhsV.Right) + Convert.ToInt16(rhsV.Right)), typeof(R)));
                                case "System.UInt16":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToUInt16(lhsV.Right) + Convert.ToUInt16(rhsV.Right)), typeof(R)));
                                case "System.Decimal":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToDecimal(lhsV.Right) + Convert.ToDecimal(rhsV.Right)), typeof(R)));
                                case "System.Double":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToDouble(lhsV.Right) + Convert.ToDouble(rhsV.Right)), typeof(R)));
                                case "System.Single":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToSingle(lhsV.Right) + Convert.ToSingle(rhsV.Right)), typeof(R)));
                                case "System.Char":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToChar(lhsV.Right) + Convert.ToChar(rhsV.Right)), typeof(R)));
                                case "System.Byte":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToByte(lhsV.Right) + Convert.ToByte(rhsV.Right)), typeof(R)));
                                case "System.String":
                                    return new EitherPair<R, L>((R)Convert.ChangeType((Convert.ToString(lhsV.Right) + Convert.ToString(rhsV.Right)), typeof(R)));
                                default:
                                    throw new InvalidOperationException("Type " + typeof(R).Name + " is not appendable.  Consider implementing the IAppendable interface.");
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Converts the Either to an enumerable of R
        /// </summary>
        /// <returns>
        /// Right: A list with one R in
        /// Left: An empty list
        /// </returns>
        public static IEnumerable<R> AsEnumerable<R, L>(this Either<R, L> self)
        {
            var res = self();
            if (res.IsRight)
                yield return res.Right;
            else
                yield break;
        }

        /// <summary>
        /// Converts the Either to an infinite enumerable
        /// </summary>
        /// <returns>
        /// Just: An infinite list of R
        /// Nothing: An empty list
        /// </returns>
        public static IEnumerable<R> AsEnumerableInfinte<R, L>(this Either<R, L> self)
        {
            var res = self();
            if (res.IsRight)
                while (true) yield return res.Right;
            else
                yield break;
        }

        /// <summary>
        /// Select
        /// </summary>
        public static Either<UR, L> Select<TR, UR, L>(
            this Either<TR, L> self,
            Func<TR, UR> selector)
        {
            return () =>
            {
                var resT = self();
                if (resT.IsLeft)
                    return new EitherPair<UR, L>(resT.Left);

                return new EitherPair<UR, L>(selector(resT.Right));
            };
        }

        /// <summary>
        /// SelectMany
        /// </summary>
        public static Either<VR, L> SelectMany<TR, UR, VR, L>(
            this Either<TR, L> self,
            Func<TR, Either<UR, L>> selector,
            Func<TR, UR, VR> projector)
        {
            return () =>
            {
                var resT = self();

                if (resT.IsLeft)
                    return new EitherPair<VR, L>(resT.Left);

                var resU = selector(resT.Right)();
                if (resU.IsLeft)
                    return new EitherPair<VR, L>(resU.Left);

                return new EitherPair<VR, L>(projector(resT.Right, resU.Right));
            };
        }

        /// <summary>
        /// Mconcat
        /// </summary>
        public static Either<R, L> Mconcat<R, L>(this IEnumerable<Either<R, L>> ms)
        {
            return () =>
            {
                var value = ms.Head();

                foreach (var m in ms.Tail())
                {
                    var res = value();
                    if (res.IsLeft)
                        return res;

                    value = value.Mappend(m);
                }
                return value();
            };
        }

        /// <summary>
        /// Memoize the result 
        /// </summary>
        public static Func<EitherPair<R, L>> Memo<R, L>(this Either<R, L> self)
        {
            var res = self();
            return () => res;
        }
    }
}