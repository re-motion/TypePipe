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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class ProxyTypeModificationContextTest
  {
    private MutableType _proxy;
    private MutableConstructorInfo _addedCtor;

    private ProxyTypeModificationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _proxy = MutableTypeObjectMother.Create();
      _addedCtor = _proxy.AddConstructor();

      _context = new ProxyTypeModificationContext (_proxy, new[] { _addedCtor.Body });
    }

    [Test]
    public void IsModified_NotModified ()
    {
      Assert.That (_context.IsModified(), Is.False);
    }

    [Test]
    public void IsModified_Constructor_AddedAttribute ()
    {
      _addedCtor.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create());

      Assert.That (_context.IsModified(), Is.True);
    }

    [Test]
    public void IsModified_Construcotr_ChangedBody ()
    {
      _addedCtor.SetBody (ctx => Expression.Empty());

      Assert.That (_context.IsModified(), Is.True);
    }

    [Test]
    public void IsModified_AddedItem ()
    {
      CheckIsModified (p => p.AddCustomAttribute());
      CheckIsModified (p => p.AddInterface());
      CheckIsModified (p => p.AddField());
      CheckIsModified (p => p.AddConstructor());
      CheckIsModified (p => p.AddMethod());
      CheckIsModified (p => p.AddProperty());
      CheckIsModified (p => p.AddEvent());
    }

    private void CheckIsModified (Action<MutableType> modificationAction)
    {
      var proxy = MutableTypeObjectMother.Create();
      var context = new ProxyTypeModificationContext (proxy, new Expression[0]);

      modificationAction (proxy);

      Assert.That (context.IsModified(), Is.True);
    }
  }
}