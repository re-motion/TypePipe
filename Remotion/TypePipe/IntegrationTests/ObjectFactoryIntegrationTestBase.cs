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
using Remotion.Development.UnitTesting;
using Remotion.ServiceLocation;
using Remotion.TypePipe.CodeGeneration;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests
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
      var participantProviders = participants.Select (p => (Func<object>) (() => p));
      var testName = GetNameForThisTest (stackFramesToSkip + 1);
      var subclassProxyBuilder = CreateSubclassProxyCreator (testName);

      var serviceLocator = new DefaultServiceLocator();
      serviceLocator.Register (typeof (IMutableTypeCodeGenerator), () => subclassProxyBuilder);
      serviceLocator.Register (typeof (IParticipant), participantProviders);

      using (new ServiceLocatorScope (serviceLocator))
        return SafeServiceLocator.Current.GetInstance<IObjectFactory>();
    }
  }
}