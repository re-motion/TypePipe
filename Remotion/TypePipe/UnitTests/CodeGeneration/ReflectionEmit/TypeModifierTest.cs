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
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class TypeModifierTest
  {
    [Test]
    public void ApplyModifications ()
    {
      var mockRepository = new MockRepository();
      var handlerFactoryMock = mockRepository.StrictMock<ISubclassProxyBuilderFactory>();

      var descriptor = UnderlyingTypeDescriptorObjectMother.Create (originalType: typeof (ClassWithMembers));
      var mutableTypePartialMock = mockRepository.PartialMock<MutableType> (
          descriptor,
          new MemberSelector (new BindingFlagsEvaluator()),
          new RelatedMethodFinder());

      var builderMock = mockRepository.StrictMock<ISubclassProxyBuilder>();
      var fakeType = ReflectionObjectMother.GetSomeType ();

      var overriddenMethod = MemberInfoFromExpressionUtility.GetMethodBaseDefinition ((ClassWithMembers obj) => obj.Method1 ());
      var overridingMethod = MemberInfoFromExpressionUtility.GetMethodBaseDefinition ((ClassWithMembers obj) => obj.Method2 ());

      using (mockRepository.Ordered ())
      {
        handlerFactoryMock.Expect (mock => mock.CreateBuilder (mutableTypePartialMock)).Return (builderMock);
        mutableTypePartialMock.Expect (mock => mock.Accept (builderMock));
        builderMock.Expect (
            mock => mock.HandleExplicitOverrides (
                Arg<IEnumerable<KeyValuePair<MethodInfo, MethodInfo>>>.List.Equal (
                    new[] { new KeyValuePair<MethodInfo, MethodInfo> (overriddenMethod, overridingMethod) })));
        builderMock.Expect (mock => mock.Build()).Return (fakeType);
      }

      mockRepository.ReplayAll();
      
      mutableTypePartialMock.AddExplicitOverride(overriddenMethod, overridingMethod);

      var typeModifier = new TypeModifier (handlerFactoryMock);
      var result = typeModifier.ApplyModifications (mutableTypePartialMock);

      mockRepository.VerifyAll ();

      Assert.That (result, Is.SameAs (fakeType));
    }
  }

  public class ClassWithMembers
  {
    public int Field1;
    public int Field2;

    public ClassWithMembers () { }
    public ClassWithMembers (int i)
    {
      Dev.Null = i;
    }

    public virtual void Method1 () { }
    public virtual void Method2 () { }
  }
}