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
using System.Runtime.CompilerServices;
using Remotion.Reflection;
using Remotion.ServiceLocation;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.ObjectFactory
{
  public abstract class ObjectFactoryIntegrationTestBase : IntegrationTestBase
  {
    [MethodImpl (MethodImplOptions.NoInlining)]
    protected IObjectFactory CreateObjectFactory (params IParticipant[] participants)
    {
      return CreateObjectFactory (participants, 1);
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    protected IObjectFactory CreateObjectFactory (IEnumerable<IParticipant> participants, int stackFramesToSkip)
    {
      var participantConfigurationID = GetType().Name;
      var testName = GetNameForThisTest (stackFramesToSkip + 1);
      var mutableTypeFactory = SafeServiceLocator.Current.GetInstance<IMutableTypeFactory>();
      var typeAssemblyContextCodeGenerator = CreateTypeAssemblyContextCodeGenerator (testName);
      var typeAssembler = new TypeAssembler (participantConfigurationID, participants, mutableTypeFactory, typeAssemblyContextCodeGenerator);
      var typeCache = new TypeCache (typeAssembler, new ConstructorFinder(), new DelegateFactory());

      return new TypePipe.ObjectFactory (typeCache);
    }
  }
}