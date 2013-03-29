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
using Remotion.ServiceLocation;
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.StrongNaming;

namespace Remotion.TypePipe.IntegrationTests
{
  public class ServiceLocatorConfigurationTest
  {
    [Test]
    public void Resolution_ObjectFactoryRegistry_SingletonScope ()
    {
      var registry1 = SafeServiceLocator.Current.GetInstance<IObjectFactoryRegistry>();
      var registry2 = SafeServiceLocator.Current.GetInstance<IObjectFactoryRegistry>();

      Assert.That (registry1, Is.Not.Null.And.SameAs (registry2));
    }

    [Test]
    public void Resolution_StrongNaming_Caches_SingletonScope ()
    {
      var typeAnalyzer1 = SafeServiceLocator.Current.GetInstance<ITypeAnalyzer>();
      var typeAnalyzer2 = SafeServiceLocator.Current.GetInstance<ITypeAnalyzer>();
      var assemblyAnalyzer1 = SafeServiceLocator.Current.GetInstance<IAssemblyAnalyzer>();
      var assemblyAnalyzer2 = SafeServiceLocator.Current.GetInstance<IAssemblyAnalyzer>();

      Assert.That (typeAnalyzer1, Is.Not.Null.And.SameAs (typeAnalyzer2));
      Assert.That (assemblyAnalyzer1, Is.Not.Null.And.SameAs (assemblyAnalyzer2));
    }

    public class DomainType { }
  }
}