// This file is part of the re-motion TypePipe project (typepipe.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-motion TypePipe is free software; you can redistribute it 
// and/or modify it under the terms of the Apache License, Version 2.0
// as published by the Apache Software Foundation.
// 
// re-motion TypePipe is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// Apache License, Version 2.0 for more details.
// 
// You should have received a copy of the Apache License, Version 2.0
// along with re-motion; if not, see http://www.apache.org/licenses.
// 
#if CLR2
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
#else
using System.Linq.Expressions;
#endif

using System.Collections.Generic;

namespace System.Dynamic.Utils {
    // Miscellaneous helpers that don't belong anywhere else
    internal static class Helpers {

        internal static T CommonNode<T>(T first, T second, Func<T, T> parent) where T : class {
            var cmp = EqualityComparer<T>.Default;
            if (cmp.Equals(first, second)) {
                return first;
            }
            var set = new Set<T>(cmp);
            for (T t = first; t != null; t = parent(t)) {
                set.Add(t);
            }
            for (T t = second; t != null; t = parent(t)) {
                if (set.Contains(t)) {
                    return t;
                }
            }
            return null;
        }

        internal static void IncrementCount<T>(T key, Dictionary<T, int> dict) {
            int count;
            dict.TryGetValue(key, out count);
            dict[key] = count + 1;
        }
    }
}
