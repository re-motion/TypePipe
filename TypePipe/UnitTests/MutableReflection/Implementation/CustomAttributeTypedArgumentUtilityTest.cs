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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomAttributeTypedArgumentUtilityTest
  {
    [Test]
    [Domain ("simple")]
    public void Unwrap_Simple ()
    {
      var typedArgument = GetTypedArgument (MethodBase.GetCurrentMethod());

      var result = CustomAttributeTypedArgumentUtility.Unwrap (typedArgument);

      Assert.That (result, Is.EqualTo ("simple"));
    }

    [Test]
    [Domain (MyEnum.C)]
    public void Unwrap_Enum ()
    {
      var typedArgument = GetTypedArgument (MethodBase.GetCurrentMethod());

      var result = CustomAttributeTypedArgumentUtility.Unwrap (typedArgument);

      Assert.That (result, Is.EqualTo (MyEnum.C));
    }

    [Test]
    [Domain (new[] { 1, 2, 3 })]
    public void Unwrap_Array ()
    {
      var typedArgument = GetTypedArgument (MethodBase.GetCurrentMethod());

      var result = CustomAttributeTypedArgumentUtility.Unwrap (typedArgument);

      Assert.That (result, Is.TypeOf<int[]>());
      Assert.That (result, Is.EqualTo (new[] { 1, 2, 3 }));
    }

    [Test]
    [Domain (new object[] { "s", 7, new[] { MyEnum.B, MyEnum.A }, typeof (int), new[] { 4, 5 } })]
    public void Unwrap_Recursive ()
    {
      var typedArgument = GetTypedArgument (MethodBase.GetCurrentMethod());

      var result = CustomAttributeTypedArgumentUtility.Unwrap (typedArgument);

      Assert.That (result, Is.TypeOf<object[]>());
      var array = ((object[]) result);
      Assert.That (array[2], Is.TypeOf<MyEnum[]>());
      Assert.That (array[4], Is.TypeOf<int[]>());
      Assert.That (array, Is.EqualTo (new object[] { "s", 7, new[] { MyEnum.B, MyEnum.A }, typeof (int), new[] { 4, 5 } }));
    }

    [Test]
    [Domain (null)]
    public void Unwrap_Null ()
    {
      var typedArgument = GetTypedArgument (MethodBase.GetCurrentMethod());

      var result = CustomAttributeTypedArgumentUtility.Unwrap (typedArgument);

      Assert.That (result, Is.Null);
    }

    [Test]
    [Domain (new[] { "1", "2", null })]
    public void Unwrap_RecursiveNull ()
    {
      var typedArgument = GetTypedArgument (MethodBase.GetCurrentMethod());

      var result = CustomAttributeTypedArgumentUtility.Unwrap (typedArgument);

      Assert.That (result, Is.EqualTo (new[] { "1", "2", null }));
    }

    private CustomAttributeTypedArgument GetTypedArgument (MethodBase method)
    {
      return CustomAttributeData.GetCustomAttributes (method)
          .Single (a => a.Constructor.DeclaringType == typeof (DomainAttribute))
          .ConstructorArguments.Single();
    }

    private class DomainAttribute : Attribute
    {
      public DomainAttribute (object obj)
      {
        Dev.Null = obj;
      }
    }

    private enum MyEnum { A, B, C }
  }
}