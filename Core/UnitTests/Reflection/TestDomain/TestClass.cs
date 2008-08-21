/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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
