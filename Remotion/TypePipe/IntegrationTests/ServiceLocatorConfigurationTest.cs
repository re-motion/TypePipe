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

namespace Remotion.TypePipe.IntegrationTests
{
  public class ServiceLocatorConfigurationTest
  {
    [Test]
    public void Resolution ()
    {
      var factory = SafeServiceLocator.Current.GetInstance<IObjectFactory>();
      Assert.That (factory, Is.Not.Null);

      var type = factory.GetAssembledType (typeof (DomainType));
      Assert.That (type, Is.Not.Null);
      Assert.That (type.Module.Name, Is.EqualTo ("<In Memory Module>"));
      Assert.That (type.Module.ScopeName, Is.StringMatching (@"TypePipe_GeneratedAssembly_\d+\.dll"));
    }

    [Test]
    public void Resolution_InstanceScope ()
    {
      var factory1 = SafeServiceLocator.Current.GetInstance<IObjectFactory>();
      var factory2 = SafeServiceLocator.Current.GetInstance<IObjectFactory>();

      Assert.That (factory1, Is.Not.SameAs (factory2));

      var type1 = factory1.GetAssembledType (typeof (DomainType));
      var type2 = factory2.GetAssembledType (typeof (DomainType));

      Assert.That (type1, Is.Not.EqualTo (type2));
      Assert.That (type1.Assembly.GetName().Name, Is.Not.EqualTo (type2.Assembly.GetName().Name));
      Assert.That (type1.Module.ScopeName, Is.Not.EqualTo (type2.Module.ScopeName));
    }

    [Test]
    [Ignore ("TODO 5222")]
    public void Resolutation_ObjectFactoryRegistry ()
    {
      var registry1 = SafeServiceLocator.Current.GetInstance<IObjectFactoryRegistry>();
      var registry2 = SafeServiceLocator.Current.GetInstance<IObjectFactoryRegistry>();

      Assert.That (registry1, Is.Not.Null.And.SameAs (registry2));
    }

    public class DomainType { }
  }
}