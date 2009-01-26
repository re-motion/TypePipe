// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// This framework is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this framework; if not, see http://www.gnu.org/licenses.
// 
using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Reflection;
using Remotion.Utilities;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class DynamicParamListImplementationTest
  {
    private DynamicParamListImplementation _implementation0;
    private DynamicParamListImplementation _implementation1;
    private DynamicParamListImplementation _implementation3;

    [SetUp]
    public void SetUp ()
    {
      _implementation0 = new DynamicParamListImplementation (new Type[0], new object[0]);
      _implementation1 = new DynamicParamListImplementation (new[] { typeof (int) }, new object[] { 1 });
      _implementation3 = new DynamicParamListImplementation (new[] { typeof (int), typeof (string), typeof (double) }, new object[] { 1, "2", 3.0 });
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The number of parameter values must match the number of parameter types.\r\n"
                                                                      + "Parameter name: parameterValues")]
    public void Initialization_Mismatch ()
    {
      new DynamicParamListImplementation (new Type[0], new object[] { 1 });
    }

    [Test]
    public void FuncType ()
    {
      Assert.That (_implementation0.FuncType, Is.Null);
      Assert.That (_implementation1.FuncType, Is.Null);
      Assert.That (_implementation3.FuncType, Is.Null);
    }

    [Test]
    public void ActionType ()
    {
      Assert.That (_implementation0.ActionType, Is.Null);
      Assert.That (_implementation1.ActionType, Is.Null);
      Assert.That (_implementation3.ActionType, Is.Null);
    }

    [Test]
    public void InvokeFunc ()
    {
      var ret = "ret";
      Assert.That (_implementation0.InvokeFunc (((Func<object>) (() => ret))), Is.SameAs (ret));
      Assert.That (_implementation1.InvokeFunc (((Func<int, object>) (i => i + ret))), Is.EqualTo (1 + ret));
      Assert.That (
          _implementation3.InvokeFunc (((Func<int, string, double, object>) ((i, s, d) => i + s + d + ret))), Is.EqualTo (1 + "2" + 3.0 + ret));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Test")]
    public void InvokeFunc_WithException ()
    {
      _implementation0.InvokeFunc (((Func<object>) (() => { throw new InvalidOperationException ("Test"); })));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument action has type System.Func`1[System.Int32] when a delegate "
                                                                          +
                                                                          "returning System.Object with the following parameter signature was expected: (System.Int32, System.String, System.Double).\r\n"
                                                                          + "Parameter name: func")]
    public void InvokeFunc_InvalidDelegate_Count ()
    {
      _implementation3.InvokeFunc (((Func<int>) (() => 5)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument action has type System.Func`4[System.Int32,System.Int32,"
                                                                          + "System.Int32,System.Object] when a delegate "
                                                                          +
                                                                          "returning System.Object with the following parameter signature was expected: (System.Int32, System.String, System.Double).\r\n"
                                                                          + "Parameter name: func")]
    public void InvokeFunc_InvalidDelegate_Types ()
    {
      _implementation3.InvokeFunc (((Func<int, int, int, object>) ((i, j, k) => 5)));
    }

    [Test]
    public void InvokeAction ()
    {
      string result = null;
      _implementation0.InvokeAction (((Action) (delegate { result = "done"; })));
      Assert.That (result, Is.EqualTo ("done"));

      _implementation1.InvokeAction (((Action<int>) (delegate (int i) { result = "done" + i; })));
      Assert.That (result, Is.EqualTo ("done1"));

      _implementation3.InvokeAction (((Action<int, string, double>) (delegate (int i, string s, double d) { result = "done" + i + s + d; })));
      Assert.That (result, Is.EqualTo ("done123"));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Test")]
    public void InvokeAction_WithException ()
    {
      _implementation0.InvokeAction (((Action) (() => { throw new InvalidOperationException ("Test"); })));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument action has type System.Action`1[System.Int32] when a delegate "
                                                                          +
                                                                          "with the following parameter signature was expected: (System.Int32, System.String, System.Double).\r\nParameter name: action"
        )]
    public void InvokeAction_InvalidDelegate_Count ()
    {
      _implementation3.InvokeAction (((Action<int>) (i => { })));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument action has type "
                                                                          + "System.Action`3[System.Int32,System.Int32,System.Int32] when a delegate "
                                                                          +
                                                                          "with the following parameter signature was expected: (System.Int32, System.String, System.Double).\r\nParameter name: action"
        )]
    public void InvokeAction_InvalidDelegate_Types ()
    {
      _implementation3.InvokeAction (((Action<int, int, int>) ((i, j, k) => { })));
    }

    [Test]
    public void GetParameterTypes ()
    {
      Assert.That (_implementation0.GetParameterTypes(), Is.Empty);
      Assert.That (_implementation1.GetParameterTypes(), Is.EqualTo (new[] { typeof (int) }));
      Assert.That (_implementation3.GetParameterTypes(), Is.EqualTo (new[] { typeof (int), typeof (string), typeof (double) }));
    }

    [Test]
    public void GetParameterTypes_ReturnsClone ()
    {
      _implementation3.GetParameterTypes()[0] = typeof (string);
      Assert.That (_implementation3.GetParameterTypes(), Is.EqualTo (new[] { typeof (int), typeof (string), typeof (double) }));
    }

    [Test]
    public void GetParameterValues ()
    {
      Assert.That (_implementation0.GetParameterValues(), Is.Empty);
      Assert.That (_implementation1.GetParameterValues(), Is.EqualTo (new object[] { 1 }));
      Assert.That (_implementation3.GetParameterValues(), Is.EqualTo (new object[] { 1, "2", 3.0 }));
    }

    [Test]
    public void GetParameterValues_ReturnsClone ()
    {
      _implementation3.GetParameterValues()[0] = 17;
      Assert.That (_implementation3.GetParameterValues(), Is.EqualTo (new object[] { 1, "2", 3.0 }));
    }
  }
}