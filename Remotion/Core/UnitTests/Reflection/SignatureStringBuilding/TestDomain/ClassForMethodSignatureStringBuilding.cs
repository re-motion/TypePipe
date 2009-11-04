// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;

namespace Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain
{
  public class ClassForMethodSignatureStringBuilding<TType1, TType2>
  {
    public void MethodWithoutParameters ()
    {
    }

    public void MethodWithParameters (string p1, DateTime p2)
    {
    }

    // ReSharper disable UnusedTypeParameter
    public void MethodWithGenericParameters<T1, T2> (string p1, DateTime p2)
        // ReSharper restore UnusedTypeParameter
    {
    }

    public T1 MethodWithUsedGenericParameters<T1, T2> (T2 p1)
    {
      return default (T1);
    }

    public TType1 MethodWithUsedGenericParametersFromOuterType (TType2 p1)
    {
      return default (TType1);
    }

    public void MethodWithClosedGenericType (ClassForMethodSignatureStringBuilding<int, string> p1)
    {
    }

    public void MethodWithGenericTypeClosedWithGenericParameters<T1> (ClassForMethodSignatureStringBuilding<T1, TType1> p1)
    {
    }

    public void MethodWithNestedType (Nested p1)
    {
    }

    public void MethodWithNestedGenericType (NestedGeneric<int> p1)
    {
    }

    public void MethodWithPartiallyClosedNestedGenericType<T1> (NestedGeneric<T1> p1, NestedGeneric<TType1> p2)
    {
    }

    public class Nested
    {
    }

    public class NestedGeneric<TNested>
    {
    }
  }
}
