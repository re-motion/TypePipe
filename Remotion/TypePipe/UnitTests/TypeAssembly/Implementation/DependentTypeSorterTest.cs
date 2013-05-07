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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.Implementation;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class DependentTypeSorterTest
  {
    private DependentTypeSorter _sorter;

    [SetUp]
    public void SetUp ()
    {
      _sorter = new DependentTypeSorter();
    }

    [Test]
    public void Sort_BaseType ()
    {
      var baseType = MutableTypeObjectMother.Create();
      var derivedType = MutableTypeObjectMother.Create (baseType: baseType);

      var result = _sorter.Sort (new[] { derivedType, baseType }.AsOneTime());

      Assert.That (result, Is.EqualTo (new[] { baseType, derivedType }));
    }

    [Test]
    public void Sort_BaseType_MutableTypeArgument ()
    {
      var typeArg = MutableTypeObjectMother.Create();
      var baseType = CustomTypeObjectMother.Create (typeArguments: new[] { typeArg });
      var derivedType = MutableTypeObjectMother.Create (baseType: baseType);

      var result = _sorter.Sort (new[] { derivedType, typeArg }.AsOneTime());

      Assert.That (result, Is.EqualTo (new[] { typeArg, derivedType }));
    }

    [Test]
    public void Sort_GetInterfaces ()
    {
      var interface_ = MutableTypeObjectMother.CreateInterface();
      var type = MutableTypeObjectMother.Create();
      type.AddInterface (interface_);

      var result = _sorter.Sort (new[] { type, interface_ });

      Assert.That (result, Is.EqualTo (new[] { interface_, type }));
    }

    [Test]
    public void Sort_GetInterfaces_MutableTypeArgument ()
    {
      var typeArg = MutableTypeObjectMother.Create();
      var interface_ = CustomTypeObjectMother.Create (attributes: TypeAttributes.Interface, typeArguments: new[] { typeArg });
      var type = MutableTypeObjectMother.Create();
      type.AddInterface (interface_);

      var result = _sorter.Sort (new[] { type, typeArg });

      Assert.That (result, Is.EqualTo (new[] { typeArg, type }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "MutableTypes must not contain cycles in their dependencies, i.e., an algorithm that recursively follows the types returned by "
        + "Type.BaseType and Type.GetInterfaces must terminate.\r\nAt least one of the following types is causing the dependency cycle: 'Ifc1', 'Ifc2'.")]
    public void Sort_ThrowsForCycles ()
    {
      var interface1 = MutableTypeObjectMother.CreateInterface ("Ifc1");
      var interface2 = MutableTypeObjectMother.CreateInterface ("Ifc2");
      interface1.AddInterface (interface2);
      interface2.AddInterface (interface1);

      _sorter.Sort (new[] { interface1, interface2 }).ForceEnumeration();
    }
  }
}