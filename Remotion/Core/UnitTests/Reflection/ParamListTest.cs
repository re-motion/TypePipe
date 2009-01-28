// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
// All rights reserved.

using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class ParamListTest
  {
    [Test]
    public void Create_0 ()
    {
      var paramList = ParamList.Create ();
      Assert.That (paramList.GetParameterTypes (), Is.Empty);
      Assert.That (paramList.GetParameterValues (), Is.Empty);
    }

    [Test]
    public void Create_0_SameAsEmpty ()
    {
      var paramList = ParamList.Create ();
      Assert.That (paramList, Is.SameAs (ParamList.Empty));
    }

    [Test]
    public void Create_1 ()
    {
      var paramList = ParamList.Create ("s");
      Assert.That (paramList.GetParameterTypes (), Is.EqualTo (new[] {typeof (string)}));
      Assert.That (paramList.GetParameterValues (), Is.EqualTo (new object[] { "s" }));
    }

    [Test]
    public void Create_10 ()
    {
      var paramList = ParamList.Create ("s", 2, 3, 4, 5.0, 6, 7, 8, DateTime.MinValue, "10");
      Assert.That (paramList.GetParameterTypes (), 
          Is.EqualTo (new[] {
              typeof (string), 
              typeof (int), 
              typeof (int), 
              typeof (int), 
              typeof (double), 
              typeof (int), 
              typeof (int), 
              typeof (int), 
              typeof (DateTime),
              typeof (string)
          }));

      Assert.That (paramList.GetParameterValues (), Is.EqualTo (new object[] { "s", 2, 3, 4, 5.0, 6, 7, 8, DateTime.MinValue, "10" }));
    }

    [Test]
    public void CreateDynamic_WithTypes ()
    {
      var paramList = ParamList.CreateDynamic (new Type[] {typeof (int)}, new object[] {1});
      Assert.That (paramList.GetParameterTypes (), Is.EqualTo (new[] {typeof (int)}));
      Assert.That (paramList.GetParameterValues (), Is.EqualTo (new object[] {1}));
    }

    [Test]
    public void CreateDynamic_WithoutTypes ()
    {
      var paramList = ParamList.CreateDynamic (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28);
      Assert.That (paramList.GetParameterTypes (), Is.EqualTo (new[] { typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), 
        typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int),
        typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int),
        typeof (int), typeof (int), typeof (int)}));
      Assert.That (paramList.GetParameterValues (), Is.EqualTo (new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 
          21, 22, 23, 24, 25, 26, 27, 28 }));
    }

    [Test]
    public void Create_Arbitrary_WithoutTypes_AndNulls ()
    {
      var paramList = ParamList.CreateDynamic (1, 2, 3, 4, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28);
      Assert.That (paramList.GetParameterTypes (), Is.EqualTo (new[] { typeof (int), typeof (int), typeof (int), typeof (int), typeof (object), 
        typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int),
        typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int), typeof (int),
        typeof (int), typeof (int), typeof (int)}));
      Assert.That (paramList.GetParameterValues (), Is.EqualTo (new object[] { 1, 2, 3, 4, null, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 
          21, 22, 23, 24, 25, 26, 27, 28 }));
    }
  }
}