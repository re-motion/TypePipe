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
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.ServiceLocation;
using Remotion.TypePipe.Configuration;
using Rhino.Mocks;

namespace Remotion.TypePipe.IntegrationTests
{
  [TestFixture]
  [Ignore]
  public class StrongNamingTest : ObjectFactoryIntegrationTestBase
  {
    private IObjectFactory _objectFactory;
    private ITypePipeConfigurationProvider _typePipeConfigurationProviderMock;

    public override void SetUp ()
    {
      base.SetUp();

      _typePipeConfigurationProviderMock = MockRepository.GenerateStrictMock<ITypePipeConfigurationProvider>();
      using (new ServiceLocatorScope (typeof (ITypePipeConfigurationProvider), () => _typePipeConfigurationProviderMock))
      {
        _objectFactory = SafeServiceLocator.Current.GetInstance<IObjectFactory>();
      }
    }

    [Test]
    public void UnsignedUnderlyingType ()
    {
      var unsignedType = GetUnsignedType("MyType");

      _objectFactory.GetAssembledType (unsignedType);
      var result = _objectFactory.CodeGenerator.FlushCodeToDisk();

      var assembly = Assembly.LoadFrom (result);
      Assert.That (assembly.GetName().GetPublicKey(), Is.Empty);
    }

    [Test]
    public void SignedUnderlyingType_RequireStrongNamed ()
    {
      _typePipeConfigurationProviderMock.Expect (x => x.RequireStrongNaming).Return (true);

      _objectFactory.GetAssembledType (typeof (DomainType));
      var result = _objectFactory.CodeGenerator.FlushCodeToDisk();

      var assembly = Assembly.LoadFrom (result);
      Assert.That (assembly.GetName().GetPublicKey(), Is.Not.Empty);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot build a signed type on the underlying unsigned type MyType.")]
    public void UnsignedUnderlyingType_RequireStrongNamed ()
    {
      var unsignedType = GetUnsignedType("MyType");

      _objectFactory.GetAssembledType (unsignedType);
      //_objectFactory.CodeGenerator.FlushCodeToDisk();
    }

    private static Type GetUnsignedType (string typeName)
    {
      var assemblyName = "test";
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");
      var typeBuilder = moduleBuilder.DefineType (typeName);
      var type = typeBuilder.CreateType();
      return type;
    }

    public class DomainType {}
  }
}