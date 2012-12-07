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
using System.Runtime.Serialization;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Serialization.Implementation;

namespace Remotion.TypePipe.UnitTests.Serialization.Implementation
{
  [TestFixture]
  public class ReflectionSerializationHelperTest
  {
    private SerializationInfo _info;
    private DomainType _instance;

    [SetUp]
    public void SetUp ()
    {
      _info = new SerializationInfo (typeof (object), new FormatterConverter ());
      _instance = new DomainType();
    }

    [Test]
    public void AddFieldValues ()
    {
      _instance.Field1 = 7;
      PrivateInvoke.SetNonPublicField (_instance, "_field4", 8);

      ReflectionSerializationHelper.AddFieldValues (_info, _instance);

      Assert.That (_info.MemberCount, Is.EqualTo (2));
      Assert.That (_info.GetValue ("<tp>Field1", typeof (int)), Is.EqualTo (7));
      Assert.That (_info.GetValue ("<tp>_field4", typeof (int)), Is.EqualTo (8));
    }

    [Test]
    public void PopulateFields ()
    {
      _info.AddValue ("<tp>Field1", 5);
      _info.AddValue ("<tp>_field4", 6);

      ReflectionSerializationHelper.PopulateFields (_info, _instance);

      Assert.That (_instance.Field1, Is.EqualTo (5));
      Assert.That (PrivateInvoke.GetNonPublicField (_instance, "_field4"), Is.EqualTo (6));
    }

    [Serializable]
    class DomainType
    {
      public int Field1;
      public static int Field2 = 0;
      [NonSerialized]
      public int Field3 = 0;
      private int _field4 = 0;

      void Dummy () { Dev.Null = _field4; }
    }
  }
}