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
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.UnitTests.MutableReflection;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MutableReflectionObjectMapTest
  {
    private MutableReflectionObjectMap _mutableReflectionObjectMap;

    [SetUp]
    public void SetUp ()
    {
      _mutableReflectionObjectMap = new MutableReflectionObjectMap();
    }

    [Test]
    public void AddMapping_GetBuilder ()
    {
      
      //var mutableType = MutableTypeObjectMother.Create();
      //var typeBuilder = 

      //_mutableReflectionObjectMap.AddMapping (reflectionEmitBuilder);
      //var result = _mutableReflectionObjectMap.GetBuilder(mutableReflectionObject);

      //Assert.That (result, Is.SameAs (reflectionEmitBuilder));
    }

    //[Test]
    //[ExpectedException (typeof (ArgumentException), ExpectedMessage = "xxx")]
    //public void AddMapping_TwiceForSameMutableReflectionObject_Throws ()
    //{
    //  var mutableReflectionObject = MutableFieldInfoObjectMother.Create ();

    //  _mutableReflectionObjectMap.AddMapping (mutableReflectionObject, ReflectionObjectMother.GetSomeField ());
    //  _mutableReflectionObjectMap.AddMapping (mutableReflectionObject, ReflectionObjectMother.GetSomeField ());
    //}

    //[Test]
    //[ExpectedException(typeof(ArgumentException), ExpectedMessage = "xxx")]
    //public void GetBuilder_NoMapping_Throws ()
    //{
    //  _mutableReflectionObjectMap.GetBuilder (MutableFieldInfoObjectMother.Create());
    //}

    
  }
}