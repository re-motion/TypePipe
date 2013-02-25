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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.SignatureBuilding;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class GenericParameterDeclarationTest
  {
    [Test]
    public void Initialization ()
    {
      var name = "parameter";
      var attributes = (GenericParameterAttributes) 7;
      Func<GenericParametersContext, Type> baseConstraintProvider = ctx => null;
      Func<GenericParametersContext, IEnumerable<Type>> interfaceConstraintsProvider = ctx => null;

      var declaration = new GenericParameterDeclaration (name, attributes, baseConstraintProvider, interfaceConstraintsProvider);

      Assert.That (declaration.Name, Is.EqualTo (name));
      Assert.That (declaration.Attributes, Is.EqualTo (attributes));
      Assert.That (declaration.BaseConstraintProvider, Is.SameAs (baseConstraintProvider));
      Assert.That (declaration.InterfaceConstraintsProvider, Is.SameAs (interfaceConstraintsProvider));
    }

    [Test]
    public void Initialization_Defaults ()
    {
      var declaration = new GenericParameterDeclaration ("name");

      Assert.That (declaration.Attributes, Is.EqualTo (GenericParameterAttributes.None));
      Assert.That (declaration.BaseConstraintProvider (null), Is.SameAs (typeof (object)));
      Assert.That (declaration.InterfaceConstraintsProvider (null), Is.Empty);
    }
  }
}