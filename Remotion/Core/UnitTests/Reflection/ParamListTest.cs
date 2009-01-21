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
  }
}