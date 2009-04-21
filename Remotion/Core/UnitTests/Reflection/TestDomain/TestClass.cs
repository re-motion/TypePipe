// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Remotion.UnitTests.Reflection.TestDomain
{

  public class TestClass
  {
    public static Type StaticMethod (Base @base)
    {
      return typeof (Base);
    }

    public static Type StaticMethod (Derived derived)
    {
      return typeof (Derived);
    }
    
    public readonly Type InvocationType;

    public TestClass (Base @base)
    {
      InvocationType = typeof (Base);
    }

    public TestClass (Derived derived)
    {
      InvocationType = typeof (Derived);
    }

    public Type InstanceMethod (Base @base)
    {
      return typeof (Base);
    }

    public Type InstanceMethod (Derived derived)
    {
      return typeof (Derived);
    }
  }
}
