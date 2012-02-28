// This file is part of the re-motion TypePipe project (typepipe.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-motion TypePipe is free software; you can redistribute it 
// and/or modify it under the terms of the Apache License, Version 2.0
// as published by the Apache Software Foundation.
// 
// re-motion TypePipe is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// Apache License, Version 2.0 for more details.
// 
// You should have received a copy of the Apache License, Version 2.0
// along with re-motion; if not, see http://www.apache.org/licenses.
// 
using System;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.TypePipe.FutureInfos.Reflection;

namespace Remotion.TypePipe.UnitTests.FutureInfos.Reflection
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

    //[Test]
    //public void SetTypeBuilder_ThrowsIfCalledMoreThanOnce ()
    //{
    //  var futureType = new FutureType();
    //  var typeBuilder = CreateTypeBuilder ("SetTypeBuilder_ThrowsIfCalledMoreThanOnce");

    //  Assert.That (() => futureType.SetTypeBuilder (typeBuilder), Throws.Nothing);
    //  Assert.That (() => futureType.SetTypeBuilder (typeBuilder), Throws.InvalidOperationException.With.Message.EqualTo(
    //    "ZZZ"));
    //}

    private TypeBuilder CreateTypeBuilder (string typeName)
    {
      return _moduleBuilder.DefineType (typeName);
    }
  }
}