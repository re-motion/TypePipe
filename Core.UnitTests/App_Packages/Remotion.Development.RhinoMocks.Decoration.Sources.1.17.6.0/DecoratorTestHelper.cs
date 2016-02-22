// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using NUnit.Framework;
using Remotion.Utilities;
using Rhino.Mocks;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.RhinoMocks.UnitTesting
{
  /// <summary>
  /// Provides functionality for testing decorator methods that do nothing else but forward to the equivalent methods on a decorated object.
  /// </summary>
  /// <typeparam name="TInterface">The type of the interface.</typeparam>
  public partial class DecoratorTestHelper<TInterface> 
      where TInterface : class
  {
    private readonly TInterface _decorator;
    private readonly TInterface _decoratedMock;

    public DecoratorTestHelper (TInterface decorator, TInterface decoratedMock)
    {
      ArgumentUtility.CheckNotNull ("decorator", decorator);
      ArgumentUtility.CheckNotNull ("decoratedMock", decoratedMock);

      _decorator = decorator;
      _decoratedMock = decoratedMock;
    }

    public void CheckDelegation<TR> (Func<TInterface, TR> action, TR fakeResult)
    {
      CheckDelegation (action, fakeResult, result => Assert.That (result, Is.EqualTo (fakeResult)));
    }

    public void CheckDelegation<TR> (Func<TInterface, TR> action, TR fakeResult, Action<TR> decoratorResultChecker)
    {
      _decoratedMock.Expect (mock => action (mock)).Return (fakeResult);
      _decoratedMock.Replay ();

      var result = action (_decorator);

      _decoratedMock.VerifyAllExpectations ();
      decoratorResultChecker (result);
    }

    public void CheckDelegation (Action<TInterface> action)
    {
      _decoratedMock.Expect (action);
      _decoratedMock.Replay ();

      action (_decorator);

      _decoratedMock.VerifyAllExpectations ();
    }

    public void CheckDelegationWithContinuation<TR> (Func<TInterface, TR> action, TR fakeResult, Action<MethodInvocation> whenCalled)
    {
      _decoratedMock.Expect (mock => action (mock)).Return (fakeResult).WhenCalled (whenCalled);
      _decoratedMock.Replay ();

      var result = action (_decorator);

      _decoratedMock.VerifyAllExpectations ();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    public void CheckDelegationWithContinuation (Action<TInterface> action, Action<MethodInvocation> whenCalled)
    {
      _decoratedMock.Expect (action).WhenCalled (whenCalled);
      _decoratedMock.Replay ();

      action (_decorator);

      _decoratedMock.VerifyAllExpectations ();
    }
  }
}