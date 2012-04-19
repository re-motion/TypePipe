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
using Remotion.TypePipe.Expressions;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class UnderlyingMethodBaseDescriptorTest
  {
    [Test]
    public void CreateOriginalBodyExpression ()
    {
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);

      var result = TestableUnderlyingMethodBaseDescriptor<MethodBase>.CreateOriginalBodyExpression (returnType, parameterDeclarations.AsOneTime());

      Assert.That (result, Is.TypeOf<OriginalBodyExpression>());
      var originalBodyExpression = ((OriginalBodyExpression) result);
      Assert.That (originalBodyExpression.Type, Is.SameAs (returnType));
      Assert.That (originalBodyExpression.Arguments, Is.EqualTo (new[] { parameterDeclarations[0].Expression, parameterDeclarations[1].Expression }));
    }

    [Test]
    public void GetMethodAttributesWithAdjustedVisibiity_NonFamilyOrAssemblyMethod ()
    {
      var method = ReflectionObjectMother.GetMethod ((DomainType obj) => obj.NonFamilyOrAssemblyMethod ());

      var attributes = TestableUnderlyingMethodBaseDescriptor<MethodBase>.GetMethodAttributesWithAdjustedVisibiity (method);

      Assert.That (attributes, Is.EqualTo (method.Attributes));
    }

    [Test]
    public void GetMethodAttributesWithAdjustedVisibiity_FamilyOrAssemblyMethod ()
    {
      var method = ReflectionObjectMother.GetMethod ((DomainType obj) => obj.FamilyOrAssemblyMethod ());
      Assert.That (method.Attributes, Is.EqualTo (MethodAttributes.FamORAssem | MethodAttributes.HideBySig));

      var attributes = TestableUnderlyingMethodBaseDescriptor<MethodBase>.GetMethodAttributesWithAdjustedVisibiity (method);

      Assert.That (attributes, Is.EqualTo (MethodAttributes.Family | MethodAttributes.HideBySig));
    }

    private abstract class DomainType
    {
      public void NonFamilyOrAssemblyMethod ()
      {
      }

      protected internal void FamilyOrAssemblyMethod ()
      {
      }
    }
  }
}