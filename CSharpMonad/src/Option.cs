﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
	/// <summary>
	/// Option monad
	/// </summary>
	public abstract class Option<T>
	{
		/// <summary>
		/// Represents a Option monad without a value
		/// </summary>
		public readonly static Option<T> Nothing = new Nothing<T>();

		// Conversion from any value to Option<T>
		// Null is always considered as Nothing
		public static implicit operator Option<T>(T value)
		{
			return value.ToOption();
		}

		/// <summary>
		/// Monad value
		/// </summary>
		public abstract T Value
		{
			get;
		}

		/// <summary>
		/// Does the monad have a value
		/// </summary>
		public abstract bool HasValue
		{
			get;
		}

		/// <summary>
		/// Get the monad's value or the default value for the type
		/// </summary>
		/// <returns></returns>
		public T GetValueOrDefault()
		{
			return HasValue ? Value : default(T);
		}

		/// <summary>
		/// Executes the delegate related to the derived Option type.
		/// </summary>
		public abstract R Match<R>(Func<R> Just, Func<R> Nothing);

		/// <summary>
		/// Executes the delegate related to the derived Option type.
		/// </summary>
		public abstract R Match<R>(Func<T, R> Just, Func<R> Nothing);

		/// <summary>
		/// Executes the delegate related to the derived Option type.
		/// </summary>
		public abstract R Match<R>(Func<R> Just, R Nothing);

		/// <summary>
		/// Executes the delegate related to the derived Option type.
		/// </summary>
		public abstract R Match<R>(Func<T, R> Just, R Nothing);

	}

	/// <summary>
	/// Option<T> monad extension methods
	/// </summary>
	public static class OptionExtensions
	{
		/// <summary>
		/// Converts this object to a Option monad.
		/// </summary>
		/// <returns>Option<T></returns>
		public static Option<T> ToOption<T>(this T self)
		{
			if (self == null)
			{
				return Option<T>.Nothing;
			}
			else
			{
				return new Just<T>(self);
			}
		}

		public static Option<R> Select<T, R>(this Option<T> self, Func<T, R> map)
		{
			return self.HasValue
				? map(self.Value).ToOption()
				: Option<R>.Nothing;
		}

		public static Option<U> SelectMany<T, U>(this Option<T> self, Func<T, Option<U>> k)
		{
			return self.HasValue
				? k(self.Value)
				: Option<U>.Nothing;
		}

		public static Option<V> SelectMany<T, U, V>(this Option<T> self, Func<T, Option<U>> k, Func<T, U, V> s)
		{
			return self.HasValue
				? s(self.Value, k(self.Value).Value).ToOption()
				: Option<V>.Nothing;
		}
	}
}
