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
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection.SignatureStringBuilding;
using Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain;

namespace Remotion.UnitTests.Reflection.SignatureStringBuilding
{
  [TestFixture]
  public class MemberSignatureStringBuilderHelperTest
  {
    private MemberSignatureStringBuilderHelper _helper;
    private StringBuilder _sb;

    [SetUp]
    public void SetUp ()
    {
      _helper = new MemberSignatureStringBuilderHelper ();
      _sb = new StringBuilder ();
    }

    [Test]
    public void AppendTypeString_SimpleType ()
    {
      _helper.AppendTypeString (_sb, typeof (void));
      
      Assert.That (_sb.ToString(), Is.EqualTo ("System.Void"));
    }

    [Test]
    public void AppendTypeString_GenericMethodParameter ()
    {
      var parameter = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithUsedGenericParameters").GetGenericArguments()[0];
      _helper.AppendTypeString (_sb, parameter);

      Assert.That (_sb.ToString (), Is.EqualTo ("[0]"));
    }

    [Test]
    public void AppendTypeString_GenericTypeParameter ()
    {
      var parameter = typeof (GenericClass<,>).GetGenericArguments ()[0];
      _helper.AppendTypeString (_sb, parameter);

      Assert.That (_sb.ToString (), Is.EqualTo ("[0/Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.GenericClass`2]"));
    }

    [Test]
    public void AppendTypeString_ClosedGenericType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericClass<int, string>));

      Assert.That (_sb.ToString(), Is.EqualTo ("Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.GenericClass`2[System.Int32,System.String]"));
    }

    [Test]
    public void AppendTypeString_GenericTypeDefinition ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericClass<,>));

      Assert.That (_sb.ToString (), Is.EqualTo ("Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.GenericClass`2"));
    }

    [Test]
    public void AppendTypeString_NestedType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericClassWithNestedType<,>.Nested));

      Assert.That (_sb.ToString (), Is.EqualTo ("Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.GenericClassWithNestedType`2+Nested"));
    }

    [Test]
    public void AppendTypeString_ClosedNestedType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericClassWithNestedType<int,string>.Nested));

      Assert.That (_sb.ToString (), Is.EqualTo (
          "Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.GenericClassWithNestedType`2+Nested[System.Int32,System.String]"));
    }

    [Test]
    public void AppendTypeString_NestedGenericType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericClassWithNestedType<,>.NestedGeneric<>));
      
      Assert.That (_sb.ToString (), Is.EqualTo ("Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.GenericClassWithNestedType`2+NestedGeneric`1"));
    }

    [Test]
    public void AppendTypeString_ClosedNestedGenericType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericClassWithNestedType<int,string>.NestedGeneric<double>));

      Assert.That (_sb.ToString (), Is.EqualTo (
          "Remotion.UnitTests.Reflection.SignatureStringBuilding.TestDomain.GenericClassWithNestedType`2+NestedGeneric`1[System.Int32,System.String,System.Double]"));
    }

    [Test]
    public void AppendSeparatedTypeStrings_None ()
    {
      _helper.AppendSeparatedTypeStrings (_sb, new Type[0]);

      Assert.That (_sb.ToString (), Is.Empty);
    }

    [Test]
    public void AppendSeparatedTypeStrings_One ()
    {
      _helper.AppendSeparatedTypeStrings (_sb, new[] { typeof (int) });

      Assert.That (_sb.ToString (), Is.EqualTo ("System.Int32"));
    }

    [Test]
    public void AppendSeparatedTypeStrings_Many ()
    {
      _helper.AppendSeparatedTypeStrings (_sb, new[] { typeof (int), typeof (double), typeof (string) });

      Assert.That (_sb.ToString (), Is.EqualTo ("System.Int32,System.Double,System.String"));
    }
  }
}
