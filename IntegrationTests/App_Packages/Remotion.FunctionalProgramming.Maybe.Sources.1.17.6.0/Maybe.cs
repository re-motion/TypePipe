// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.FunctionalProgramming
{
  /// <summary>
  /// Provides non-generic helper methods for the <see cref="Maybe{T}"/> type.
  /// </summary>
  static partial class Maybe
  {
    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> instance for the given value, which can be <see langword="null" />.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="valueOrNull"/>.</typeparam>
    /// <param name="valueOrNull">The value. Can be <see langword="null" />, in which case <see cref="Maybe{T}.Nothing"/> is returned.</param>
    /// <returns><see cref="Maybe{T}.Nothing"/> if <paramref name="valueOrNull"/> is <see langword="null" />; otherwise an instance of 
    /// <see cref="Maybe{T}"/> that encapsulates the given <paramref name="valueOrNull"/>.</returns>
    public static Maybe<T> ForValue<T> (T valueOrNull)
    {
      return new Maybe<T> (valueOrNull);
    }

    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> instance for the given <paramref name="nullableValue"/>, unwrapping a nullable value type.
    /// </summary>
    /// <typeparam name="T">The underlying type of the <paramref name="nullableValue"/>.</typeparam>
    /// <param name="nullableValue">
    ///   The nullable value. Can be <see langword="null"/>, in which case <see cref="Maybe{T}.Nothing"/> is returned.
    /// </param>
    /// <returns><see cref="Maybe{T}.Nothing"/> if <paramref name="nullableValue"/> is <see langword="null" />; otherwise an instance of 
    /// <see cref="Maybe{T}"/> that encapsulates the underlying value of <paramref name="nullableValue"/>.</returns>
    public static Maybe<T> ForValue<T> (T? nullableValue) where T : struct
    {
      return nullableValue.HasValue ? new Maybe<T> (nullableValue.Value) : Maybe<T>.Nothing;
    }

    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> instance for the given value, which can be <see langword="null"/>, if a boolean condition evaluates to
    /// <see langword="true"/>. If it evaluates to <see langword="false"/>, <see cref="Maybe{T}.Nothing"/> is returned.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="valueIfTrue"/>.</typeparam>
    /// <param name="condition">The condition to check. If <see langword="false" />, <see cref="Maybe{T}.Nothing"/> is returned.</param>
    /// <param name="valueIfTrue">The value. Can be <see langword="null"/>, in which case <see cref="Maybe{T}.Nothing"/> is returned.</param>
    /// <returns>
    /// 	<see cref="Maybe{T}.Nothing"/> if <paramref name="valueIfTrue"/> is <see langword="null"/> or <paramref name="condition"/> is 
    /// 	<see langword="false" />; otherwise an instance of <see cref="Maybe{T}"/> that encapsulates the given <paramref name="valueIfTrue"/>.
    /// </returns>
    public static Maybe<T> ForCondition<T> (bool condition, T valueIfTrue)
    {
      return ForValue (valueIfTrue).Where (v => condition);
    }

    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> instance for the given value, unwrapping a nullable value type, if a boolean condition evaluates to
    /// <see langword="true"/>. If it evaluates to <see langword="false"/>, <see cref="Maybe{T}.Nothing"/> is returned.
    /// </summary>
    /// <typeparam name="T">The underlying type of the <paramref name="nullableValueIfTrue"/>.</typeparam>
    /// <param name="condition">The condition to check. If <see langword="false" />, <see cref="Maybe{T}.Nothing"/> is returned.</param>
    /// <param name="nullableValueIfTrue">
    ///   The nullable value. Can be <see langword="null"/>, in which case <see cref="Maybe{T}.Nothing"/> is returned.
    ///   </param>
    /// <returns>
    /// 	<see cref="Maybe{T}.Nothing"/> if <paramref name="nullableValueIfTrue"/> is <see langword="null"/> or <paramref name="condition"/> is 
    /// 	<see langword="false" />; otherwise an instance of <see cref="Maybe{T}"/> that encapsulates the underlying value of 
    /// 	<paramref name="nullableValueIfTrue"/>.
    /// </returns>
    public static Maybe<T> ForCondition<T> (bool condition, T? nullableValueIfTrue) where T : struct
    {
      return condition ? ForValue (nullableValueIfTrue) : Maybe<T>.Nothing;
    }

    /// <summary>
    /// Enumerates the values of a number of <see cref="Maybe{T}"/> instances. <see cref="Maybe{T}"/> instances that have no values are ignored.
    /// </summary>
    /// <typeparam name="T">The value type of the <see cref="Maybe{T}"/> values.</typeparam>
    /// <param name="maybeValues">The maybe instances to enumerate the values of. <see cref="Maybe{T}"/> instances that have no values are ignored.</param>
    /// <returns>An enumerable sequence containing all non-<see langword="null" /> values</returns>
    public static IEnumerable<T> EnumerateValues<T> (IEnumerable<Maybe<T>> maybeValues)
    {
      return maybeValues.Where (v => v.HasValue).Select (v => v.ValueOrDefault ());
    }

    /// <summary>
    /// Enumerates the values of a number of <see cref="Maybe{T}"/> instances. <see cref="Maybe{T}"/> instances that have no values are ignored.
    /// </summary>
    /// <typeparam name="T">The value type of the <see cref="Maybe{T}"/> values.</typeparam>
    /// <param name="maybeValues">The maybe instances to enumerate the values of. <see cref="Maybe{T}"/> instances that have no values are ignored.</param>
    /// <returns>An enumerable sequence containing all non-<see langword="null" /> values</returns>
    public static IEnumerable<T> EnumerateValues<T> (params Maybe<T>[] maybeValues)
    {
      return EnumerateValues ((IEnumerable<Maybe<T>>) maybeValues);
    }
  }

  /// <summary>
  /// Encapsulates a value that may be <see langword="null" />, providing helpful methods to avoid <see langword="null" /> checks.
  /// </summary>
  [Serializable]
  partial struct Maybe<T>
  {
    public static readonly Maybe<T> Nothing = new Maybe<T> ();

    private readonly T _value;
    private readonly bool _hasValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="Maybe{T}"/> struct.
    /// </summary>
    /// <param name="value">
    /// The value. If the value is <see langword="null" />, the created instance will compare equal to <see cref="Nothing"/>.
    /// </param>
    public Maybe (T value)
    {
      _value = value;
      // ReSharper disable CompareNonConstrainedGenericWithNull
      _hasValue = value != null;
      // ReSharper restore CompareNonConstrainedGenericWithNull
    }

    /// <summary>
    /// Gets a value indicating whether this instance has a value.
    /// </summary>
    /// <value>
    /// 	<see langword="true"/> if this instance has a value; otherwise, <see langword="false"/>.
    /// </value>
    public bool HasValue
    {
      get { return _hasValue; }
    }

    /// <summary>
    /// Provides a human-readable representation of this instance.
    /// </summary>
    /// <returns>
    /// A human-readable representation of this instance.
    /// </returns>
    public override string ToString ()
    {
      return string.Format ("{0} ({1})", (HasValue ? "Value: " + _value : "Nothing"), typeof (T).Name);
    }

    /// <summary>
    /// Gets the value held by this instance, or the default value of <typeparamref name="T"/> if this instance does not have a value.
    /// </summary>
    /// <returns>The value held by this instance, or the default value of <typeparamref name="T"/> if this instance does not have a value.</returns>
    public T ValueOrDefault ()
    {
      return ValueOrDefault (default (T));
    }

    /// <summary>
    /// Gets the value held by this instance, or the <paramref name="defaultValue"/> if this instance does not have a value.
    /// </summary>
    /// <param name="defaultValue">The default value returned if this instance does not have a value.</param>
    /// <returns>The value held by this instance, or the <paramref name="defaultValue"/> if this instance does not have a value.</returns>
    public T ValueOrDefault (T defaultValue)
    {
      return _hasValue ? _value : defaultValue;
    }

    /// <summary>
    /// Gets the value held by this instance. An exception is thrown if this instance does not have a value.
    /// </summary>
    /// <returns>The value held by this instance. An exception is thrown if this instance does not have a value.</returns>
    /// <exception cref="InvalidOperationException">The <see cref="HasValue"/> property is <see langword="false" />.</exception>
    public T Value ()
    {
      if (!_hasValue)
        throw new InvalidOperationException ("Maybe instance does not have a value.");
      return _value;
    }

    /// <summary>
    /// Executes the specified action if this instance has a value. Otherwise, the action is not performed.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This instance.</returns>
    public Maybe<T> Do (Action<T> action)
    {
      if (_hasValue)
        action (_value);

      return this;
    }

    /// <summary>
    /// Executes the specified action if this instance has a value. Otherwise, a different action is performed.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="otherwise">The action to execute if this instance doesn't have a value.</param>
    /// <returns>This instance.</returns>
    public Maybe<T> Do (Action<T> action, Action otherwise)
    {
      return Do (action).OtherwiseDo (otherwise);
    }

    /// <summary>
    /// Executes the specified action if this instance doesn't have a value. If it does, the action is not performed.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>This instance.</returns>
    public Maybe<T> OtherwiseDo (Action action)
    {
      if (!_hasValue)
        action();

      return this;
    }

    /// <summary>
    /// Selects another value from this instance. If this instance does not have a value, the selected value is <see cref="Nothing"/>.
    /// Otherwise, the selected value is retrieved via a selector function.
    /// </summary>
    /// <typeparam name="TR">The type of the value to be selected.</typeparam>
    /// <param name="selector">The selector function. This function is only executed if this instance has a value.</param>
    /// <returns><see cref="Nothing"/> if this instance has no value or <paramref name="selector"/> returns <see langword="null" />; 
    /// otherwise, a new <see cref="Maybe{T}"/> instance holding the value returned by <paramref name="selector"/>.
    /// </returns>
    public Maybe<TR> Select<TR> (Func<T, TR> selector)
    {
      ArgumentUtility.CheckNotNull ("selector", selector);

      if (_hasValue)
        return Maybe.ForValue (selector (_value));
      else
        return Maybe<TR>.Nothing;
    }

    /// <summary>
    /// Combines a <see cref="Maybe{T}"/> value with another <see cref="Maybe{T}"/> value, returning <see cref="Nothing"/> unless both
    /// <see cref="Maybe{T}"/> instances have a value.
    /// </summary>
    /// <typeparam name="TOther">The type stored by the other maybe instance to combine this instance with.</typeparam>
    /// <typeparam name="TResult">The type of the combined values.</typeparam>
    /// <param name="otherMaybeSelector">A selector function that returns the other <see cref="Maybe{T}"/> instance to combine this instance with. 
    /// This function is only executed if this instance has a value.</param>
    /// <param name="resultSelector">A function selecting the resulting value from the value of this instance and the value of the other 
    /// <see cref="Maybe{T}"/> instance.</param>
    /// <returns><see cref="Nothing"/> if this instance does not have a value, <paramref name="otherMaybeSelector"/> returns <see cref="Nothing"/>,
    /// or <paramref nane="resultSelector"/> returns <see langword="null" />. Otherwise, the result of <paramref name="resultSelector"/> applied
    /// to the value held by this instance and the value held by the <see cref="Maybe{T}"/> instance returned by <paramref name="otherMaybeSelector"/>.
    /// </returns>
    /// <remarks>
    /// This method enables LINQ-style queries with multiple from clauses.
    /// <example>
    /// <code>
    /// var r = from s in Maybe.ForValue (stringValue)
    ///         from i in Maybe.ForValue (intValue)
    ///         let j = s.Length + i
    ///         where j &gt; 100
    ///         select new { i, s };
    /// r.Do (tuple => Console.WriteLine (tuple.i + "/" + tuple.s), () => Console.WriteLine ("Nothing!"));
    /// </code>
    /// </example>
    /// </remarks>
    public Maybe<TResult> SelectMany<TOther, TResult> (Func<T, Maybe<TOther>> otherMaybeSelector, Func<T, TOther, TResult> resultSelector) 
    {
      if (!_hasValue)
        return Maybe<TResult>.Nothing;

      var otherMaybe = otherMaybeSelector (_value);
      if (!otherMaybe._hasValue)
        return Maybe<TResult>.Nothing;

      return Maybe.ForValue (resultSelector (_value, otherMaybe._value));
    }
   

    /// <summary>
    /// Selects a nullable value from this instance. If this instance does not have a value, the selected value is <see cref="Nothing"/>.
    /// Otherwise, the selected value is retrieved via a selector function.
    /// </summary>
    /// <typeparam name="TR">The type of the value to be selected.</typeparam>
    /// <param name="selector">The selector function. This function is only executed if this instance has a value. Its return value is unwrapped
    /// into the underlying type.</param>
    /// <returns><see cref="Nothing"/> if this instance has no value or <paramref name="selector"/> returns <see langword="null" />; 
    /// otherwise, a new <see cref="Maybe{T}"/> instance holding the non-nullable value returned by <paramref name="selector"/>.
    /// </returns>
    public Maybe<TR> Select<TR> (Func<T, TR?> selector) where TR : struct
    {
      ArgumentUtility.CheckNotNull ("selector", selector);

      if (_hasValue)
        return Maybe.ForValue (selector (_value));
      else
        return Maybe<TR>.Nothing;
    }

    /// <summary>
    /// Selects a new value if this instance does not have a value.
    /// If it already has a value, that value is retained.
    /// </summary>
    /// <param name="selector">The selector function. This function is only executed if this instance doesn't have a value.</param>
    /// <returns>This <see cref="Maybe{T}"/> instance if it holds a value. Otherwise, a new <see cref="Maybe{T}"/> instance holding the result of 
    /// the <paramref name="selector"/> (or <see cref="Nothing"/> if the selector returned <see langword="null" />).
    /// </returns>
    public Maybe<T> OtherwiseSelect (Func<T> selector)
    {
      ArgumentUtility.CheckNotNull ("selector", selector);

      if (_hasValue)
        return this;
      else
        return Maybe.ForValue (selector());
    }

    /// <summary>
    /// Checks the given predicate, returning this instance if the predicate is <see langword="true" />, or <see cref="Nothing"/> if the predicate is 
    /// <see langword="false" />. If this instance does not have a value, <see cref="Nothing"/> is immediately returned and the predicate is not
    /// evaluated.
    /// </summary>
    /// <param name="predicate">The predicate to check. This is only evaluated if this instance has a value.</param>
    /// <returns><see cref="Nothing"/> if this instance does not have a value or the <paramref name="predicate"/> returns <see langword="false" />;
    /// otherwise, this instance.</returns>
    public Maybe<T> Where (Func<T, bool> predicate)
    {
      return _hasValue && predicate (_value) ? this : Nothing;
    }
  }
}