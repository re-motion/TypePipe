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
using Remotion.Utilities;

namespace Remotion.TypePipe.Caching
{
  // TODO 5057: When updating to 4.0, replace usages with ConcurrentDictionary from .NET framework.
  public class ConcurrentDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
  {
    private readonly object _lock = new object();

    private readonly Dictionary<TKey, TValue> _dictionary;

    // For testing only.
    public ConcurrentDictionary ()
        : this (EqualityComparer<TKey>.Default)
    {
    }

    public ConcurrentDictionary (IEqualityComparer<TKey> comparer)
    {
      ArgumentUtility.CheckNotNull ("comparer", comparer);

      _dictionary = new Dictionary<TKey, TValue> (comparer);
    }

    public int Count
    {
      get
      {
        lock (_lock)
        {
          return _dictionary.Count;
        }
      }
    }

    public TValue this [TKey key]
    {
      get
      {
        lock (_lock)
        {
          return _dictionary[key];
        }
      }
    }

    public bool ContainsKey (TKey key)
    {
      lock (_lock)
      {
        return _dictionary.ContainsKey (key);
      }
    }

    public void Add (TKey key, TValue value)
    {
      lock (_lock)
      {
        _dictionary.Add (key, value);
      }
    }

    public bool TryGetValue (TKey key, out TValue value)
    {
      lock (_lock)
      {
        return _dictionary.TryGetValue (key, out value);
      }
    }

    public TValue GetOrAdd (TKey key, Func<TKey, TValue> valueProvider)
    {
      lock (_lock)
      {
        TValue result;
        if (!_dictionary.TryGetValue (key, out result))
        {
          result = valueProvider (key);
          _dictionary.Add (key, result);
        }
        return result;
      }
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator ()
    {
      throw new NotImplementedException ();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
    {
      throw new NotImplementedException ();
    }
  }
}