// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class PropertyTest
  {
    [Test]
    public void PropertyInfo()
    {
      var propertyInfo = Properties<PropertyTestClass>.Get (x => x.Name).PropertyInfo;
      Assert.That (propertyInfo, Is.EqualTo (typeof (PropertyTestClass).GetProperty ("Name")));
    }

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
