// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.TypePipe.Implementation;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class DynamicParamListTest
  {
    delegate void TestAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20);
    delegate TResult TestFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, in T19, in T20, out TResult> (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20);

    private DynamicParamList _implementation0;
    private DynamicParamList _implementation1;
    private DynamicParamList _implementation3;
    private DynamicParamList _implementation20;

    [SetUp]
    public void SetUp ()
    {
      _implementation0 = new DynamicParamList (new Type[0], new object[0]);
      _implementation1 = new DynamicParamList (new[] { typeof (int) }, new object[] { 1 });
      _implementation3 = new DynamicParamList (new[] { typeof (int), typeof (string), typeof (double) }, new object[] { 1, "2", 3.0 });
      _implementation20 = new DynamicParamList (
          new[]
          {
              typeof (int), typeof (string), typeof (double), typeof (bool), typeof (bool),
              typeof (bool), typeof (bool), typeof (bool), typeof (bool), typeof (bool), 
              typeof (bool), typeof (bool), typeof (bool), typeof (bool), typeof (bool),
              typeof (bool), typeof (bool), typeof (bool), typeof (bool), typeof (byte)
          },
          new object[] { 1, "2", 3.0, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, (byte) 20 });
    }

    [Test]
    public void Initialization_Mismatch ()
    {
      Assert.That (
          () => new DynamicParamList (new Type[0], new object[] { 1 }),
          Throws.ArgumentException
              .With.Message.EqualTo ("The number of parameter values must match the number of parameter types.\r\nParameter name: parameterValues"));
    }

    [Test]
    public void FuncType ()
    {
      Assert.That (_implementation0.FuncType, Is.SameAs (typeof (Func<object>)));
      Assert.That (_implementation1.FuncType, Is.SameAs (typeof (Func<int, object>)));
      Assert.That (_implementation3.FuncType, Is.SameAs (typeof (Func<int, string, double, object>)));
      Assert.That (_implementation3.FuncType, Is.SameAs (typeof (Func<int, string, double, object>)));
      Assert.That (_implementation20.FuncType, Is.Not.Null);
      Assert.That (_implementation20.FuncType, Is.SameAs (_implementation20.FuncType));
    }

    [Test]
    public void FuncType_ExceedsMaximumParameterCount ()
    {
      var implementation21 = new DynamicParamList (new Type[21], new object[21]);
      Assert.That (
          () => implementation21.FuncType,
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Getting the FuncType for a DynamicParamList is only supported for up to 20 parameters but the DynamicParamList was initialized with 21 parameters."));
    }

    [Test]
    public void ActionType ()
    {
      Assert.That (_implementation0.ActionType, Is.SameAs (typeof (Action)));
      Assert.That (_implementation1.ActionType, Is.SameAs (typeof (Action<int>)));
      Assert.That (_implementation3.ActionType, Is.SameAs (typeof (Action<int, string, double>)));
      Assert.That (_implementation20.ActionType, Is.Not.Null);
      Assert.That (_implementation20.ActionType, Is.SameAs (_implementation20.ActionType));
    }

    [Test]
    public void ActionType_ExceedsMaximumParameterCount ()
    {
      var implementation21 = new DynamicParamList (new Type[21], new object[21]);
      Assert.That (
          () => implementation21.ActionType,
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Getting the ActionType for a DynamicParamList is only supported for up to 20 parameters but the DynamicParamList was initialized with 21 parameters."));
    }

    [Test]
    public void InvokeFunc ()
    {
      var ret = "ret";
      Assert.That (_implementation0.InvokeFunc (((Func<object>) (() => ret))), Is.SameAs (ret));
      Assert.That (_implementation1.InvokeFunc (((Func<int, object>) (i => i + ret))), Is.EqualTo (1 + ret));
      Assert.That (
          _implementation3.InvokeFunc (((Func<int, string, double, object>) ((i, s, d) => i + s + d + ret))), Is.EqualTo (1 + "2" + 3.0 + ret));
      Assert.That (
          _implementation20.InvokeFunc (
              ((TestFunc<int, string, double, bool, bool, bool, bool, bool, bool, bool,
                  bool, bool, bool, bool, bool, bool, bool, bool, bool, byte, object>)
                  ((i, s, d, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15, b16, b17, b18, b19, @byte) => i + s + d + @byte + ret))),
          Is.EqualTo ("12320ret"));
    }

    [Test]
    public void InvokeFunc_WithException ()
    {
      Assert.That (
          () => _implementation0.InvokeFunc (((Func<object>) (() => { throw new InvalidOperationException ("Test"); }))),
          Throws.InvalidOperationException
              .With.Message.EqualTo ("Test"));
    }

    [Test]
    public void InvokeFunc_InvalidDelegate_Count ()
    {
      Assert.That (
          () => _implementation3.InvokeFunc (((Func<int>) (() => 5))),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'func' has type 'System.Func`1[System.Int32]' when a delegate "
                  +"returning System.Object with the following parameter signature was expected: (System.Int32, System.String, System.Double)."
                  + "\r\nParameter name: func"));
    }

    [Test]
    public void InvokeFunc_InvalidDelegate_Types ()
    {
      Assert.That (
          () => _implementation3.InvokeFunc (((Func<int, int, int, object>) ((i, j, k) => 5))),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'func' has type 'System.Func`4[System.Int32,System.Int32,System.Int32,System.Object]' when a delegate "
                  + "returning System.Object with the following parameter signature was expected: (System.Int32, System.String, System.Double)."
                  + "\r\nParameter name: func"));
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

      _implementation20.InvokeFunc (
          ((TestAction<int, string, double, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, byte>)
              ((i, s, d, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15, b16, b17, b18, b19, @byte) => { result = "done" + i + s + d + @byte; })));
      Assert.That (result, Is.EqualTo ("done12320"));
    }

    [Test]
    public void InvokeAction_WithException ()
    {
      Assert.That (
          () => _implementation0.InvokeAction (((Action) (() => { throw new InvalidOperationException ("Test"); }))),
          Throws.InvalidOperationException
              .With.Message.EqualTo ("Test"));
    }

    [Test]
    public void InvokeAction_InvalidDelegate_Count ()
    {
      Assert.That (
          () => _implementation3.InvokeAction (((Action<int>) (i => { }))),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'action' has type 'System.Action`1[System.Int32]' when a delegate with the following parameter signature was expected: "
                  + "(System.Int32, System.String, System.Double)."
                  + "\r\nParameter name: action"));
    }

    [Test]
    public void InvokeAction_InvalidDelegate_Types ()
    {
      Assert.That (
          () => _implementation3.InvokeAction (((Action<int, int, int>) ((i, j, k) => { }))),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'action' has type 'System.Action`3[System.Int32,System.Int32,System.Int32]' " +
                  "when a delegate with the following parameter signature was expected: (System.Int32, System.String, System.Double)."
                  + "\r\nParameter name: action"));
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
