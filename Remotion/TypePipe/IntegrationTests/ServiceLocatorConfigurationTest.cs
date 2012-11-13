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

using NUnit.Framework;
using Remotion.ServiceLocation;
using Remotion.TypePipe;

namespace TypePipe.IntegrationTests
{
  public class ServiceLocatorConfigurationTest
  {
    [Test]
    public void Resolution ()
    {
      Assert.That (() => SafeServiceLocator.Current.GetInstance<IObjectFactory>(), Throws.Nothing);
      var factory = SafeServiceLocator.Current.GetInstance<IObjectFactory>();
      Assert.That (factory, Is.Not.Null);

      var instance = factory.CreateInstance<DomainType>();
      Assert.That (instance, Is.Not.Null);
      Assert.That (instance.GetType().Module.Name, Is.EqualTo ("<In Memory Module>"));
    }

    public class DomainType { }
  }
}