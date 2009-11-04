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
    public void PropertySetGetTest ()
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

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The body of the passed expression is not a MemberExpression, the passed expression does therefore not represent a property.")]
    public void Ctor_ExpressionNotAMemberTest ()
    {
      Properties<PropertyTestClass>.Get (x => x.Foo());
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The passed expression does not represent a property.")]
    public void Ctor_ExpressionNotAPropertyTest ()
    {
      Properties<PropertyTestClass>.Get (x => x.SomeText);
    }

  }


  public class PropertyTestClass {
    public string SomeText;

    public string Name { get; set; }

    public int Number { get; set; }

    public int Foo ()
    {
      return 1;
    }
  }
}
