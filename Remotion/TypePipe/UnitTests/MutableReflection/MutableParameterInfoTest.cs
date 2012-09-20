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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableParameterInfoTest
  {
    [Test]
    public void Initialization ()
    {
      var parameterType = ReflectionObjectMother.GetSomeType ();
      var name = "parameterName";
      var attributes = ParameterAttributes.Out;
      var descriptor = UnderlyingParameterInfoDescriptorObjectMother.CreateForNew (parameterType, name, attributes);
      var member = ReflectionObjectMother.GetSomeMember ();
      var position = 4711;

      var mutableParameter = new MutableParameterInfo (member, position, descriptor);

      Assert.That (mutableParameter.Member, Is.SameAs (member));
      Assert.That (mutableParameter.Position, Is.EqualTo (position));
      Assert.That (mutableParameter.ParameterType, Is.SameAs (parameterType));
      Assert.That (mutableParameter.Name, Is.EqualTo (name));
      Assert.That (mutableParameter.Attributes, Is.EqualTo (attributes));
    }
    
    //[Test]
    //public void GetCustomAttributeData ()
    //{
    //  var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((MutableParameterInfoTest obj) => obj.Method (""));
    //  var parameter = method.GetParameters().Single();
    //  var mutableParameter = MutableParameterInfoObjectMother.Create()

    //  var result = mutableField.GetCustomAttributeData ();

    //  Assert.That (result.Select (a => a.Constructor.DeclaringType), Is.EquivalentTo (new[] { typeof (AbcAttribute) }));
    //}

    //[Test]
    //public void GetCustomAttributeData_Lazy ()
    //{
    //  var field = NormalizingMemberInfoFromExpressionUtility.GetField (() => Field);
    //  var mutableField = MutableFieldInfoObjectMother.CreateForExisting (originalField: field);

    //  var result1 = mutableField.GetCustomAttributeData ();
    //  var result2 = mutableField.GetCustomAttributeData ();

    //  Assert.That (result1, Is.SameAs (result2));
    //}

    private void Method ([Abc] string parameter) { }

    public class AbcAttribute : Attribute { }
  }
}