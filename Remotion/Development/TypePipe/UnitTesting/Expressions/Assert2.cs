// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Collections;
using System.Linq;
using Microsoft.Scripting.Ast;

namespace Remotion.Development.TypePipe.UnitTesting.Expressions
{
  public static class Assert2
  {
    public static void AreEqual (object expected, object actual, string message)
    {
      if (expected == actual)
        return;

      if (expected == null)
        throw Throw (message);

      if (expected.Equals (actual))
        return;

      var expectedEnumerable = expected as IEnumerable;
      var actualEnumerable = actual as IEnumerable;
      if (expectedEnumerable != null && actualEnumerable != null && expectedEnumerable.Cast<object>().SequenceEqual (actualEnumerable.Cast<object>()))
        return;

      Throw (message);
    }

    public static void IsNull (Expression actual, string message)
    {
      AreEqual (null, actual, message);
    }

    public static void IsTrue (bool actual, string message)
    {
      AreEqual (true, actual, message);
    }

    public static void IsInstanceOf<T> (object actual, string message)
    {
      if (!(actual is T))
        Throw (message);
    }

    private static Exception Throw (string message)
    {
      throw new Exception (message);
    }
  }
}