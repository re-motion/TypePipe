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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class PropertyTest
  {

    [Test]
    public void PropertyTest1 ()
    {
      TestProperty(Properties<PropertyTestClass>.Get (x => x.Name),"zip");
      TestProperty (Properties<PropertyTestClass>.Get (x => x.Number), 123);
    }

    private void TestProperty<TProperty> (Property<PropertyTestClass, TProperty> property, TProperty value) {
      PropertyTestClass test = new PropertyTestClass();
      Assert.That (property.Get (test), Is.Not.EqualTo (value));
      property.Set (test, value);
      Assert.That (property.Get (test), Is.EqualTo (value));
    }
  }


  class PropertyTestClass {
    private int _number;

    public string Name { get; set; }

    public int Number
    {
      get { return _number; }
      set { _number = value; }
    }
  }
}