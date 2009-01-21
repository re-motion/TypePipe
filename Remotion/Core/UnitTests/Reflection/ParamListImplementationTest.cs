// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
// All rights reserved.

using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class ParamListImplementationTest
  {
    private ParamListImplementation _implementation0;
    private ParamListImplementation<int> _implementation1;
    private ParamListImplementation<int, string, double> _implementation3;

    [SetUp]
    public void SetUp ()
    {
      _implementation0 = new ParamListImplementation ();
      _implementation1 = new ParamListImplementation<int> (1);
      _implementation3 = new ParamListImplementation<int, string, double> (1, "2", 3.0);
    }

    [Test]
    public void FuncType ()
    {
      Assert.That (_implementation0.FuncType, Is.EqualTo (typeof (Func<object>)));
      Assert.That (_implementation1.FuncType, Is.EqualTo (typeof (Func<int, object>)));
      Assert.That (_implementation3.FuncType, Is.EqualTo (typeof (Func<int, string, double, object>)));
    }

    [Test]
    public void ActionType ()
    {
      Assert.That (_implementation0.ActionType, Is.EqualTo (typeof (Action)));
      Assert.That (_implementation1.ActionType, Is.EqualTo (typeof (Action<int>)));
      Assert.That (_implementation3.ActionType, Is.EqualTo (typeof (Action<int, string, double>)));
    }

    [Test]
    public void ExecuteFunc ()
    {
      var ret = "ret";
      Assert.That (_implementation0.ExecuteFunc (((Func<object>) (() => ret))), Is.SameAs (ret));
      Assert.That (_implementation1.ExecuteFunc (((Func<int, object>) (i => i + ret))), Is.EqualTo (1 + ret));
      Assert.That (_implementation3.ExecuteFunc (((Func<int, string, double, object>) ((i, s, d) => i + s + d + ret))), Is.EqualTo (1 + "2" + 3.0 + ret));
    }

    [Test]
    public void ExecuteAction ()
    {
      string result = null;
      _implementation0.ExecuteAction (((Action) (delegate { result = "done"; })));
      Assert.That (result, Is.EqualTo ("done"));

      _implementation1.ExecuteAction (((Action<int>) (delegate (int i) { result = "done" + i; })));
      Assert.That (result, Is.EqualTo ("done1"));

      _implementation3.ExecuteAction (((Action<int, string, double>) (delegate (int i, string s, double d) { result = "done" + i + s + d; })));
      Assert.That (result, Is.EqualTo ("done123"));
    }

    [Test]
    public void GetParameterTypes ()
    {
      Assert.That (_implementation0.GetParameterTypes (), Is.Empty);
      Assert.That (_implementation1.GetParameterTypes (), Is.EqualTo (new[] { typeof (int) }));
      Assert.That (_implementation3.GetParameterTypes (), Is.EqualTo (new[] { typeof (int), typeof (string), typeof (double) }));
    }

    [Test]
    public void GetParameterValues ()
    {
      Assert.That (_implementation0.GetParameterValues (), Is.Empty);
      Assert.That (_implementation1.GetParameterValues (), Is.EqualTo (new object[] { 1 }));
      Assert.That (_implementation3.GetParameterValues (), Is.EqualTo (new object[] { 1, "2", 3.0 }));
    }
  }
}