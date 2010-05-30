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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class DynamicMethodBasedMethodCallerTest
  {
// ReSharper disable MemberCanBePrivate.Global
    public interface IInterfaceWithMethods
// ReSharper restore MemberCanBePrivate.Global
    {
      string ImplicitInterfaceMethod (string value);
      string ExplicitInterfaceMethod (string value);
    }

    private class ClassWithMethods : IInterfaceWithMethods
    {
      public static string StaticValue { get; set; }

      public static string PublicStaticMethod (string value)
      {
        StaticValue = value;
        return value;
      }

      private static string NonPublicStaticMethod (string value)
      {
        StaticValue = value;
        return value;
      }

      public string InstanceValue { get; set; }

      public string PublicInstanceMethod (string value)
      {
        InstanceValue = value;
        return value;
      }

      private string NonPublicInstanceMethod (string value)
      {
        InstanceValue = value;
        return value;
      }

      public string ImplicitInterfaceMethod (string value)
      {
        InstanceValue = value;
        return value;
      }

      string IInterfaceWithMethods.ExplicitInterfaceMethod (string value)
      {
        InstanceValue = value;
        return value;
      }
    }

    [Test]
    public void GetMethodDelegate_PublicInstanceMethod ()
    {
      Type declaringType = typeof (ClassWithMethods);
      var methodInfo = declaringType.GetMethod ("PublicInstanceMethod", BindingFlags.Public | BindingFlags.Instance);

      var @delegate = (Func<ClassWithMethods, string, string>) DynamicMethodBasedMethodCallerFactory.CreateMethodCallerDelegate (
          methodInfo, typeof (Func<ClassWithMethods, string, string>));

      var obj = new ClassWithMethods();
      Assert.That (@delegate (obj, "TheValue"), Is.EqualTo ("TheValue"));
      Assert.That (obj.InstanceValue, Is.EqualTo ("TheValue"));
    }

    [Test]
    public void GetMethodDelegate_NonPublicInstanceMethod ()
    {
      Type declaringType = typeof (ClassWithMethods);
      var methodInfo = declaringType.GetMethod ("NonPublicInstanceMethod", BindingFlags.NonPublic | BindingFlags.Instance);

      var @delegate = (Func<ClassWithMethods, string, string>) DynamicMethodBasedMethodCallerFactory.CreateMethodCallerDelegate (
          methodInfo, typeof (Func<ClassWithMethods, string, string>));

      var obj = new ClassWithMethods();
      Assert.That (@delegate (obj, "TheValue"), Is.EqualTo ("TheValue"));
      Assert.That (obj.InstanceValue, Is.EqualTo ("TheValue"));
    }

    [Test]
    public void GetMethodDelegate_ImplicitInterfaceMethod ()
    {
      Type declaringType = typeof (IInterfaceWithMethods);
      var methodInfo = declaringType.GetMethod ("ImplicitInterfaceMethod", BindingFlags.Public | BindingFlags.Instance);

      var @delegate = (Func<object, string, string>) DynamicMethodBasedMethodCallerFactory.CreateMethodCallerDelegate (
          methodInfo, typeof (Func<object, string, string>));

      var obj = new ClassWithMethods();
      Assert.That (@delegate (obj, "TheValue"), Is.EqualTo ("TheValue"));
      Assert.That (obj.InstanceValue, Is.EqualTo ("TheValue"));
    }

    [Test]
    public void GetMethodDelegate_ExplicitInterfaceMethod ()
    {
      Type declaringType = typeof (IInterfaceWithMethods);
      var methodInfo = declaringType.GetMethod ("ExplicitInterfaceMethod", BindingFlags.Public | BindingFlags.Instance);

      var @delegate = (Func<object, string, string>) DynamicMethodBasedMethodCallerFactory.CreateMethodCallerDelegate (
          methodInfo, typeof (Func<object, string, string>));

      var obj = new ClassWithMethods();
      Assert.That (@delegate (obj, "TheValue"), Is.EqualTo ("TheValue"));
      Assert.That (obj.InstanceValue, Is.EqualTo ("TheValue"));
    }

    [Test]
    public void GetMethodDelegate_PublicStaticMethod ()
    {
      Type declaringType = typeof (ClassWithMethods);
      var methodInfo = declaringType.GetMethod ("PublicStaticMethod", BindingFlags.Public | BindingFlags.Static);

      var @delegate = (Func<ClassWithMethods, string, string>) DynamicMethodBasedMethodCallerFactory.CreateMethodCallerDelegate (
          methodInfo, typeof (Func<ClassWithMethods, string, string>));

      Assert.That (@delegate (null, "TheValue"), Is.EqualTo ("TheValue"));
      Assert.That (ClassWithMethods.StaticValue, Is.EqualTo ("TheValue"));
    }

    [Test]
    public void GetMethodDelegate_NonPublicStaticMethod ()
    {
      Type declaringType = typeof (ClassWithMethods);
      var methodInfo = declaringType.GetMethod ("NonPublicStaticMethod", BindingFlags.NonPublic | BindingFlags.Static);

      var @delegate = (Func<ClassWithMethods, string, string>) DynamicMethodBasedMethodCallerFactory.CreateMethodCallerDelegate (
          methodInfo, typeof (Func<ClassWithMethods, string, string>));

      Assert.That (@delegate (null, "TheValue"), Is.EqualTo ("TheValue"));
      Assert.That (ClassWithMethods.StaticValue, Is.EqualTo ("TheValue"));
    }
  }
}