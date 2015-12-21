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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;

[assembly: CustomAttributeDataRetrieverTest.Abc ("assembly")]
[module: CustomAttributeDataRetrieverTest.Abc ("module")]

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomAttributeDataRetrieverTest
  {
    private CustomAttributeDataRetriever _retriever;

    [SetUp]
    public void SetUp ()
    {
      _retriever = new CustomAttributeDataRetriever();
    }

    [Test]
    [Abc ("member")]
    public void GetCustomAttributeData_Member ()
    {
      MemberInfo member = MethodBase.GetCurrentMethod();

      var result = _retriever.GetCustomAttributeData (member).ToArray();

      CheckContainsAttribute (result, typeof (AbcAttribute), "member");
      CheckContainsAttribute (result, typeof (TestAttribute));
    }

    [Test]
    public void GetCustomAttributeData_Parameter ()
    {
      var parameter = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => MethodWithParameter (7)).GetParameters().Single();

      var result = _retriever.GetCustomAttributeData (parameter);

      CheckContainsAttribute (result, typeof (AbcAttribute), "parameter");
    }

    [Test]
    public void GetCustomAttributeData_Assembly ()
    {
      var assembly = GetType().Assembly;

      var result = _retriever.GetCustomAttributeData (assembly).ToArray();

      CheckContainsAttribute (result, typeof (AbcAttribute), "assembly");
      CheckContainsAttribute (result, typeof (AssemblyTitleAttribute), "Remotion TypePipe Library Unit Tests");
    }

    [Test]
    public void GetCustomAttributeData_Module ()
    {
      var module = GetType().Module;

      var result = _retriever.GetCustomAttributeData (module);

      CheckContainsAttribute (result, typeof (AbcAttribute), "module");
    }

    private void CheckContainsAttribute (IEnumerable<ICustomAttributeData> attributes, Type attributeType, params object[] constructorArgs)
    {
      var matchinAttributes = attributes.Count (a => a.Type == attributeType && a.ConstructorArguments.SequenceEqual (constructorArgs));
      Assert.That (matchinAttributes, Is.EqualTo (1), "Wrong number of matching attributes.");
    }

    void MethodWithParameter ([Abc ("parameter")] int i) { Dev.Null = i; }

    public class AbcAttribute : Attribute
    {
      public AbcAttribute (string s) { String = s; }
      public string String { get; set; }
    }
  }
}