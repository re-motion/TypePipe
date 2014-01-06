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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection.BodyBuilding;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class GenericParameterConversionsTest : TypeAssemblerIntegrationTestBase
  {
    private MethodInfo _baseMethod;
    private Type _t1;
    private Type _t2;

    public override void SetUp ()
    {
      base.SetUp();

      _baseMethod = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType o) => o.GenericMethod<C, D> (null, null));
      var genericParameters = _baseMethod.GetGenericArguments ();
      _t1 = genericParameters[0];
      _t2 = genericParameters[1];
    }

    [Test]
    public void ToGenericParameter_FromGenericParameter ()
    {
      CheckAssignability (_t1, _t1);

      CheckConversion (_t1, _t2); // TODO 5480: Should be implicitly convertible.
      CheckConversion (_t2, _t1);
    }

    [Test]
    public void ToReferenceType_FromGenericParameter ()
    {
      CheckConversion (typeof (object), _t1);// TODO 5480: Should be implicitly convertible.
      CheckConversion (typeof (A), _t1);// TODO 5480: Should be implicitly convertible.
      CheckConversion (typeof (B), _t1);// TODO 5480: Should be implicitly convertible.

      CheckInvalidConversion (typeof (C), _t1);
      CheckInvalidConversion (typeof (D), _t1);
    }

    [Test]
    public void ToGenericParameter_FromReferenceType ()
    {
      CheckConversion (_t1, typeof (object));
      CheckConversion (_t1, typeof (A));
      CheckConversion (_t1, typeof (B));

      CheckInvalidConversion (_t1, typeof (C));
      CheckInvalidConversion (_t1, typeof (D));
    }

    [Test]
    public void ToValueType_FromGenericType ()
    {
      SkipSavingAndPeVerification();
      CheckInvalidConversion (typeof (int), _t1);
    }

    [Test]
    public void ToGenericParameter_FromValueType ()
    {
      SkipSavingAndPeVerification();
      CheckInvalidConversion (_t1, typeof (int));
    }

    private void CheckAssignability (Type toType, Type fromType)
    {
      Func<MethodBodyModificationContext, Expression> bodyProvider = ctx =>
      {
        var variable = Expression.Variable (toType);
        var assignment = Expression.Assign (variable, Expression.Default (fromType));
        var conversion = Expression.Convert (Expression.Default (fromType), toType);

        return Expression.Block (new[] { variable }, assignment, conversion);
      };
      CreateType (bodyProvider);
    }

    private void CheckConversion (Type toType, Type fromType)
    {
      Assert.That (
          () => Expression.Assign (Expression.Variable (toType), Expression.Default (fromType)),
          Throws.ArgumentException.With.Message.Contains ("cannot be used for assignment"));

      Func<MethodBodyModificationContext, Expression> bodyProvider =
          ctx => Expression.Convert (Expression.Default (fromType), toType);
      CreateType (bodyProvider);
    }

    private void CheckInvalidConversion (Type toType, Type fromType)
    {
      Assert.That (
          () => Expression.Assign (Expression.Variable (toType), Expression.Default (fromType)),
          Throws.ArgumentException.With.Message.Contains ("cannot be used for assignment"));

      Assert.That (
          () => Expression.Convert (Expression.Default (fromType), toType),
          Throws.InvalidOperationException.With.Message.StartsWith ("No coercion operator is defined between types"));
    }

    private void CreateType (Func<MethodBodyModificationContext, Expression> bodyProvider)
    {
      var type = AssembleType<DomainType> (p => p.GetOrAddOverride (_baseMethod).SetBody (bodyProvider));
      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (() => instance.GenericMethod<C, D> (null, null), Throws.Nothing);
    }

    public class DomainType
    {
      public virtual void GenericMethod<T1, T2> (T1 t1, T2 t2)
          where T1 : B
          where T2 : T1 {}
    }

    public class A {}
    public class B : A {}
    public class C : B {}
    public class D : C {}
  }
}