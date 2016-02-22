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
using JetBrains.Annotations;
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.FunctionalProgramming
{
  /// <summary>
  /// Provides a set of <see langword="static"/> methods for querying objects that implement <see cref="IEnumerable{T}"/>.
  /// </summary>
  static partial class EnumerableExtensions
  {
    /// <summary>
    /// Returns the first element of a sequence
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <typeparam name="TException">Type type of the exception returned by <paramref name="createEmptySequenceException"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to return the first element of. Must not be <see langword="null" />.</param>
    /// <param name="createEmptySequenceException">
    /// This callback is invoked if the sequence is empty. The returned exception is then thrown to indicate this error. Must not be <see langword="null" />.
    /// </param>
    /// <returns>The first element in the specified sequence.</returns>
    public static TSource First<TSource, TException> (this IEnumerable<TSource> source, Func<TException> createEmptySequenceException)
        where TException: Exception
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("createEmptySequenceException", createEmptySequenceException);

      using (IEnumerator<TSource> enumerator = source.GetEnumerator())
      {
        if (enumerator.MoveNext())
          return enumerator.Current;
      }

      throw createEmptySequenceException();
    }

    /// <summary>
    /// Returns the first element in a sequence that satisfies a specified condition.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <typeparam name="TException">Type type of the exception returned by <paramref name="createNoMatchingElementException"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to return an element of. Must not be <see langword="null" />.</param>
    /// <param name="predicate">A function to test each element for a condition. Must not be <see langword="null" />.</param>
    /// <param name="createNoMatchingElementException">
    /// This callback is invoked if the sequence is empty or no element satisfies the condition in <paramref name="predicate"/>. 
    /// The returned exception is then thrown to indicate this error. Must not be <see langword="null" />.
    /// </param>
    /// <returns>The first element in the sequence that passes the test in the specified predicate function.</returns>
    public static TSource First<TSource, TException> (
        this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TException> createNoMatchingElementException)
        where TException: Exception
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      ArgumentUtility.CheckNotNull ("createNoMatchingElementException", createNoMatchingElementException);

      foreach (TSource current in source)
      {
        if (predicate (current))
          return current;
      }

      throw createNoMatchingElementException();
    }

    /// <summary>
    /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <typeparam name="TException">Type type of the exception returned by <paramref name="createEmptySequenceException"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to return the single element of. Must not be <see langword="null" />.</param>
    /// <param name="createEmptySequenceException">
    /// This callback is invoked if the sequence is empty. 
    /// The returned exception is then thrown to indicate this error. Must not be <see langword="null" />.
    /// </param>
    /// <returns>The single element in the specified sequence.</returns>
    /// <exception cref="InvalidOperationException">InvalidOperationException The specified sequence contains more than one element.</exception>
    public static TSource Single<TSource, TException> (this IEnumerable<TSource> source, Func<TException> createEmptySequenceException)
        where TException: Exception
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("createEmptySequenceException", createEmptySequenceException);

      TSource result = default (TSource);
      bool isElementFound = false;
      foreach (TSource current in source)
      {
        if (isElementFound)
          throw new InvalidOperationException ("Sequence contains more than one element.");

        isElementFound = true;
        result = current;
      }

      if (isElementFound)
        return result;

      throw createEmptySequenceException();
    }

    /// <summary>
    /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <typeparam name="TException">Type type of the exception returned by <paramref name="createNoMatchingElementException"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to return a single element of. Must not be <see langword="null" />.</param>
    /// <param name="predicate">A function to test each element for a condition. Must not be <see langword="null" />.</param>
    /// <param name="createNoMatchingElementException">
    /// This callback is invoked if the sequence is empty or no element satisfies the condition in <paramref name="predicate"/>. 
    /// The returned exception is then thrown to indicate this error. Must not be <see langword="null" />.
    /// </param>
    /// <returns>The single element in the specified sequence.</returns>
    /// <exception cref="InvalidOperationException">InvalidOperationException The specified sequence contains more than one element.</exception>
    public static TSource Single<TSource, TException> (
        this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TException> createNoMatchingElementException)
        where TException: Exception
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      ArgumentUtility.CheckNotNull ("createNoMatchingElementException", createNoMatchingElementException);

      TSource result = default (TSource);
      bool isElementFound = false;
      foreach (TSource current in source)
      {
        if (predicate (current))
        {
          if (isElementFound)
            throw new InvalidOperationException ("Sequence contains more than one matching element.");

          isElementFound = true;
          result = current;
        }
      }

      if (isElementFound)
        return result;

      throw createNoMatchingElementException();
    }

    /// <summary>
    /// Generates a sequence of elements from the <paramref name="source"/> element by applying the specified next-element function, 
    /// adding elements to the sequence while the current element satisfies the specified condition.
    /// </summary>
    /// <typeparam name="TSource">The type of the <paramref name="source"/> element.</typeparam>
    /// <param name="source">The object to be transformed into a sequence.</param>
    /// <param name="nextElementSelector">A function to retrieve the next element in the sequence. Must not be <see langword="null" />.</param>
    /// <param name="predicate">A function to test each element for a condition. Must not be <see langword="null" />.</param>
    /// <returns>
    /// A collection of elements containing the <paramref name="source"/> and all subsequent elements where each element satisfies a specified condition.
    /// </returns>
    public static IEnumerable<TSource> CreateSequence<TSource> (this TSource source, Func<TSource, TSource> nextElementSelector, Func<TSource, bool> predicate)
    {
      ArgumentUtility.CheckNotNull ("nextElementSelector", nextElementSelector);
      ArgumentUtility.CheckNotNull ("predicate", predicate);

      for (TSource current = source; predicate (current); current = nextElementSelector (current))
        yield return current;
    }

    /// <summary>
    /// Generates a sequence of elements from the <paramref name="source"/> element by applying the specified next-element function, 
    /// adding elements to the sequence while the current element is not <see langword="null" />.
    /// </summary>
    /// <typeparam name="TSource">The type of the <paramref name="source"/> element.</typeparam>
    /// <param name="source">The object to be transformed into a sequence.</param>
    /// <param name="nextElementSelector">A function to retrieve the next element in the sequence. Must not be <see langword="null" />.</param>
    /// <returns>
    /// A sequence of elements containing the <paramref name="source"/> and all subsequent elements 
    /// until the <paramref name="nextElementSelector"/> returns <see langword="null" />.
    /// </returns>
    public static IEnumerable<TSource> CreateSequence<TSource> (this TSource source, Func<TSource, TSource> nextElementSelector)
        where TSource : class
    {
      ArgumentUtility.CheckNotNull ("nextElementSelector", nextElementSelector);

      return CreateSequence (source, nextElementSelector, e => e != null);
    }

    /// <summary>
    /// Generates a sequence of elements from the <paramref name="source"/> element by applying the specified next-element function, 
    /// adding elements to the sequence while the current element is not <see langword="null" />. 
    /// If a cycle is detected based on the <see cref="EqualityComparer{TSource}.Default"/> comparer, an exception is thrown.
    /// </summary>
    /// <typeparam name="TSource">The type of the <paramref name="source"/> element.</typeparam>
    /// <typeparam name="TException">Type type of the exception returned by <paramref name="createCycleFoundException"/>.</typeparam>
    /// <param name="source">The object to be transformed into a sequence.</param>
    /// <param name="nextElementSelector">A function to retrieve the next element in the sequence. Must not be <see langword="null" />.</param>
    /// <param name="createCycleFoundException">
    /// This callback is invoked if a cycle is detected within the sequence.
    /// The returned exception is then thrown to indicate this error. Must not be <see langword="null" />.
    /// </param>
    /// <returns>
    /// A sequence of elements containing the <paramref name="source"/> and all subsequent elements 
    /// until the <paramref name="nextElementSelector"/> returns <see langword="null" />.
    /// </returns>
    public static IEnumerable<TSource> CreateSequenceWithCycleCheck<TSource, TException> (
        this TSource source,
        Func<TSource, TSource> nextElementSelector,
        Func<TSource, TException> createCycleFoundException)
        where TSource : class
        where TException : Exception
    {
      return CreateSequenceWithCycleCheck (source, nextElementSelector, e => e != null, EqualityComparer<TSource>.Default, createCycleFoundException);
    }

    /// <summary>
    /// Generates a sequence of elements from the <paramref name="source"/> element by applying the specified next-element function, 
    /// adding elements to the sequence while the current element satisfies the specified condition. If a cycle is detected, an exception is thrown.
    /// </summary>
    /// <typeparam name="TSource">The type of the <paramref name="source"/> element.</typeparam>
    /// <typeparam name="TException">Type type of the exception returned by <paramref name="createCycleFoundException"/>.</typeparam>
    /// <param name="source">The object to be transformed into a sequence.</param>
    /// <param name="nextElementSelector">A function to retrieve the next element in the sequence. Must not be <see langword="null" />.</param>
    /// <param name="predicate">A function to test each element for a condition. Must not be <see langword="null" />.</param>
    /// <param name="equalityComparer">
    /// The <see cref="IEqualityComparer{TSource}"/> used when checking if an element was already returned as part of the sequence.
    /// Can be <see langword="null" /> to indicate default behavior.
    /// </param>
    /// <param name="createCycleFoundException">
    /// This callback is invoked if a cycle is detected within the sequence.
    /// The returned exception is then thrown to indicate this error. Must not be <see langword="null" />.
    /// </param>
    /// <returns>
    /// A collection of elements containing the <paramref name="source"/> and all subsequent elements where each element satisfies a specified condition.
    /// </returns>
    public static IEnumerable<TSource> CreateSequenceWithCycleCheck<TSource, TException> (
        this TSource source,
        Func<TSource, TSource> nextElementSelector,
        Func<TSource, bool> predicate,
        [CanBeNull] IEqualityComparer<TSource> equalityComparer,
        Func<TSource, TException> createCycleFoundException)
        where TException : Exception
    {
      ArgumentUtility.CheckNotNull ("nextElementSelector", nextElementSelector);
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      ArgumentUtility.CheckNotNull ("createCycleFoundException", createCycleFoundException);

      var chainMembers = new HashSet<TSource> (equalityComparer);
      Func<TSource, TSource> nextElementSelectorWithCycleCheck = element =>
      {
        if (chainMembers.Contains (element))
          throw createCycleFoundException (element);
        chainMembers.Add (element);

        return nextElementSelector (element);
      };

      return CreateSequence (source, nextElementSelectorWithCycleCheck, predicate);
    }

    /// <summary>
    /// Determines whether two enumerable sequences contain the same set of elements without regarding the order or number of elements.
    /// This method constructs a <see cref="HashSet{T}"/> from <paramref name="sequence1"/> and then calls <see cref="HashSet{T}.SetEquals"/>.
    /// The <see cref="EqualityComparer{T}.Default"/> equality comparer is used to check elements for equality.
    /// </summary>
    /// <typeparam name="T">The element type of the compared sequences.</typeparam>
    /// <param name="sequence1">The first sequence.</param>
    /// <param name="sequence2">The second sequence.</param>
    /// <returns><see langword="true" /> if all elements of <paramref name="sequence1"/> are present in <paramref name="sequence2"/> and all
    /// elements of <paramref name="sequence2"/> are present in <paramref name="sequence1"/>. Order and number of elements are not compared.</returns>
    /// <exception cref="ArgumentNullException">One of the sequences is <see langword="null" />.</exception>
    public static bool SetEquals<T> (this IEnumerable<T> sequence1, IEnumerable<T> sequence2)
    {
      ArgumentUtility.CheckNotNull ("sequence1", sequence1);
      ArgumentUtility.CheckNotNull ("sequence2", sequence2);

      return new HashSet<T> (sequence1).SetEquals (sequence2);
    }

    /// <summary>
    /// Combines two sequences into a single sequence of <see cref="Tuple{T1,T2}"/> values.
    /// </summary>
    /// <typeparam name="T1">The item type of the first sequence.</typeparam>
    /// <typeparam name="T2">The item type of the second sequence.</typeparam>
    /// <param name="first">The first sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <returns>
    /// A "zipped" sequence, consisting of the combined elements of the <paramref name="first"/> and the <paramref name="second"/> sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">One of the parameters is null.</exception>
    /// <remarks>
    /// If the input sequences do not have the same number of arguments, the result sequence will have as many arguments as the smaller input sequence.
    /// </remarks>
    public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2> (this IEnumerable<T1> first, IEnumerable<T2> second)
    {
      ArgumentUtility.CheckNotNull ("first", first);
      ArgumentUtility.CheckNotNull ("second", second);

      return first.Zip (second, Tuple.Create);
    }

    /// <summary>
    /// Interleaves the elements of two sequences.
    /// </summary>
    /// <typeparam name="T">The item type of the sequences.</typeparam>
    /// <param name="first">The first sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <returns>
    /// An "interleaved" sequence, consisting of alternating elements of the <paramref name="first"/> and the <paramref name="second"/> sequence.
    /// </returns>
    /// <remarks>
    /// If the input sequences do not have the same number of arguments, the remaining items in the longer sequence will be appended.
    /// </remarks>
    public static IEnumerable<T> Interleave<T> (this IEnumerable<T> first, IEnumerable<T> second)
    {
      using (var enumerator1 = first.GetEnumerator ())
      using (var enumerator2 = second.GetEnumerator ())
      {
        bool firstHasMore;
        bool secondHasMore;

        while ((firstHasMore = enumerator1.MoveNext()) | (secondHasMore = enumerator2.MoveNext()))
        {
          if (firstHasMore)
            yield return enumerator1.Current;

          if (secondHasMore)
            yield return enumerator2.Current;
        }
      }
    }

    /// <summary>
    /// Returns an object of type <see cref="ICollection{T}"/> that has the same items as the source <see cref="IEnumerable{T}"/>. If the source 
    /// <see cref="IEnumerable{T}"/> already implements <see cref="ICollection{T}"/>, the same instance is returned without any copying taking place.
    /// </summary>
    /// <typeparam name="T">The item type of the <paramref name="source"/> sequence (and the result <see cref="ICollection{T}"/>).</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to be returned as an <see cref="ICollection{T}"/> instance.</param>
    /// <returns><paramref name="source"/> if that object implements <see cref="ICollection{T}"/>, otherwise a new collection with the same items
    /// as <paramref name="source"/>.</returns>
    public static ICollection<T> ConvertToCollection<T> (this IEnumerable<T> source)
    {
      ArgumentUtility.CheckNotNull ("source", source);

      var collection = source as ICollection<T>;
      if (collection != null)
        return collection;

      return source.ToList();
    }

    /// <summary>
    /// Returns a sequence that has the same elements as the given <paramref name="source"/> sequence, with the given <paramref name="item"/> appended
    /// to the end. This method is similar to <see cref="Enumerable.Concat{TSource}"/>, but allows a single item to be appended to the sequence.
    /// </summary>
    /// <typeparam name="T">The element type of the <paramref name="source"/> sequence.</typeparam>
    /// <param name="source">The sequence to which a new item should be appended.</param>
    /// <param name="item">The item to be appended.</param>
    /// <returns>
    /// A lazy sequence that first enumerates the items from the <paramref name="source"/> sequence, then yields the <paramref name="item"/>.
    /// </returns>
    public static IEnumerable<T> Concat<T> (this IEnumerable<T> source, T item)
    {
      ArgumentUtility.CheckNotNull ("source", source);

      return source.Concat (EnumerableUtility.Singleton (item));
    }

    /// <summary>
    /// Works like <see cref="Enumerable.SingleOrDefault{TSource}(System.Collections.Generic.IEnumerable{TSource})"/> but throws a custom
    /// exception if the sequence contains more than one element.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="createMultipleElementsException">The exception provider.</param>
    /// <returns></returns>
    public static TSource SingleOrDefault<TSource, TException> (this IEnumerable<TSource> source, Func<TException> createMultipleElementsException)
        where TException : Exception
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("createMultipleElementsException", createMultipleElementsException);

      using (var enumerator = source.GetEnumerator())
      {
        if (!enumerator.MoveNext())
          return default (TSource);

        var element = enumerator.Current;
        if (enumerator.MoveNext())
          throw createMultipleElementsException();

        return element;
      }
    }
    
    /// <summary>
    /// Works like <see cref="Enumerable.SingleOrDefault{TSource}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,bool})"/>
    /// but throws a custom exception if the sequence contains more than one matching element.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="predicate">The predicate applied to the sequence.</param>
    /// <param name="createMultipleMatchingElementsException">The exception provider.</param>
    /// <returns></returns>
    public static TSource SingleOrDefault<TSource, TException> (
        this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TException> createMultipleMatchingElementsException)
        where TException : Exception
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      ArgumentUtility.CheckNotNull ("createMultipleMatchingElementsException", createMultipleMatchingElementsException);
      
      return SingleOrDefault (source.Where (predicate), createMultipleMatchingElementsException);
    }

    /// <summary>
    /// Returns a sequence that lazily applies a side effect to each item of a source sequence.
    /// </summary>
    /// <typeparam name="T">The item type of the <see cref="IEnumerable{T}"/> sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="sideEffect">The side effect to apply to each item.</param>
    /// <returns>A sequence containing the same items as the <paramref name="source"/> sequence that lazily applies the given 
    /// <paramref name="sideEffect"/> to each item while the sequence is enumerated.</returns>
    /// <remarks>
    /// This method is used to associate a side effect with an enumerable sequence. One use case is to add consistency checks to the items of a 
    /// sequence that are executed when the sequence is enumerated. 
    /// </remarks>
    public static IEnumerable<T> ApplySideEffect<T> (this IEnumerable<T> source, Action<T> sideEffect)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("sideEffect", sideEffect);

      foreach (var item in source)
      {
        sideEffect (item);
        yield return item;
      }
    }
  }
}