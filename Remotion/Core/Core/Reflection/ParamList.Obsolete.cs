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

namespace Remotion.Reflection
{
  [Obsolete ("Moved to TypePipe. (Version 1.15.7.0)")]
  internal abstract class ParamList
  {
    public static ParamList Empty
    {
      get { return null; }
    }

    public static ParamList Create<A1> (A1 a1)
    {
      return null;
    }

    public static ParamList Create<A1, A2> (A1 a1, A2 a2)
    {
      return null;
    }

    public static ParamList Create<A1, A2, A3> (A1 a1, A2 a2, A3 a3)
    {
      return null;
    }

    public static ParamList Create<A1, A2, A3, A4> (A1 a1, A2 a2, A3 a3, A4 a4)
    {
      return null;
    }

    public static ParamList Create<A1, A2, A3, A4, A5> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
    {
      return null;
    }

    public static ParamList Create<A1, A2, A3, A4, A5, A6> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6)
    {
      return null;
    }

    public static ParamList Create<A1, A2, A3, A4, A5, A6, A7> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7)
    {
      return null;
    }
  }
}