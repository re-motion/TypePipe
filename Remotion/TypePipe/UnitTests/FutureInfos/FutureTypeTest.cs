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
using Remotion.TypePipe.FutureInfos;

namespace Remotion.TypePipe.UnitTests.FutureInfos
{
  [TestFixture]
  public class FutureTypeTest
  {
    private ModuleBuilder _moduleBuilder;

    [TestFixtureSetUp]
    public void GenerateAssembly ()
    {
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName ("FutureTypeTest"), AssemblyBuilderAccess.RunAndSave);
      _moduleBuilder = assemblyBuilder.DefineDynamicModule ("FutureTypeTest.dll");
    }

    //[Test]
    //public void Initialization ()
    //{
    //  // TODO
    //}

    [Test]
    public void FutureTypeIsAType ()
    {
      Assert.That (new FutureType(), Is.AssignableTo<Type>());
    }

    [Test]
    public void SetTypeBuilder_ThrowsIfCalledMoreThanOnce ()
    {
      var futureType = new FutureType();
      var typeBuilder = CreateTypeBuilder ("SetTypeBuilder_ThrowsIfCalledMoreThanOnce");

      Assert.That (() => futureType.SetTypeBuilder (typeBuilder), Throws.Nothing);
      Assert.That (() => futureType.SetTypeBuilder (typeBuilder),
        Throws.InvalidOperationException.With.Message.EqualTo ("TypeBuilder already set"));
    }

    private TypeBuilder CreateTypeBuilder (string typeName)
    {
      return _moduleBuilder.DefineType (typeName);
    }
  }
}