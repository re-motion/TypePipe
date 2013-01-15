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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class ProxyTypeModelFactoryTest
  {
    private ProxyTypeModelFactory _factory;

    private Type _domainType;

    [SetUp]
    public void SetUp ()
    {
      _factory = new ProxyTypeModelFactory();

      _domainType = typeof (DomainType);
    }

    [Test]
    public void CreateProxy ()
    {
      var result = _factory.CreateProxyType (_domainType);

      Assert.That (result.BaseType, Is.SameAs (_domainType));
      Assert.That (result.Name, Is.EqualTo (@"DomainType_Proxy1"));
      Assert.That (result.Namespace, Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.Implementation"));
      Assert.That (result.FullName, Is.EqualTo (@"Remotion.TypePipe.UnitTests.MutableReflection.Implementation.DomainType_Proxy1"));
      Assert.That (result.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.BeforeFieldInit));
    }

    [Test]
    public void CreateProxy_UniqueNames ()
    {
      var result1 = _factory.CreateProxyType (_domainType);
      var result2 = _factory.CreateProxyType (_domainType);

      Assert.That (result1.Name, Is.Not.EqualTo (result2.Name));
    }

    [Test]
    public void CreateProxy_NullNamespace ()
    {
      var baseTypeStub = MockRepository.GenerateStub<Type>();
      baseTypeStub.Stub (stub => stub.Name).Return ("abc");
      baseTypeStub.Stub (stub => stub.Namespace).Return (null);
      baseTypeStub.Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything)).Return (new ConstructorInfo[0]);

      var result = _factory.CreateProxyType (baseTypeStub);

      Assert.That (result.Namespace, Is.Null);
      Assert.That (result.FullName, Is.EqualTo (@"abc_Proxy1"));
    }

    [Test]
    public void CreateProxy_Serializable ()
    {
      var result = _factory.CreateProxyType (typeof (SerializableType));

      Assert.That (result.IsSerializable, Is.True);
    }

    [Test]
    public void CreateProxy_CopiesAccessibleInstanceConstructors ()
    {
      var result = _factory.CreateProxyType (_domainType);

      Assert.That (result.AddedConstructors, Has.Count.EqualTo (1));

      var ctor = result.AddedConstructors.Single();
      Assert.That (ctor.IsStatic, Is.False);
      Assert.That (ctor.IsFamilyOrAssembly, Is.True);

      var parameters = ctor.GetParameters();
      Assert.That (parameters, Has.Length.EqualTo (1));
      Assert.That (parameters[0].ParameterType, Is.SameAs (typeof (int)));
      Assert.That (parameters[0].Name, Is.EqualTo ("i"));

      var baseCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (7));
      var expectedBody = new OriginalBodyExpression (baseCtor, typeof (void), ctor.ParameterExpressions.Cast<Expression>());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, ctor.Body);
    }

    public class DomainType
    {
      static DomainType() { }

      protected internal DomainType (int i) { Dev.Null = i; }
      internal DomainType (string inaccessible) { Dev.Null = inaccessible; }
    }

    [Serializable]
    public class SerializableType { }
  }
}