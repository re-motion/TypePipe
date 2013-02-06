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
using System.Reflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  public static class CustomEventInfoObjectMother
  {
    public static CustomEventInfo Create (
        CustomType declaringType = null,
        string name = "Event",
        EventAttributes attributes = (EventAttributes) 7,
        MethodInfo addMethod = null,
        MethodInfo removeMethod = null,
        MethodInfo raiseMethod = null,
        IEnumerable<ICustomAttributeData> customAttributes = null)
    {
      declaringType = declaringType ?? CustomTypeObjectMother.Create();
      addMethod = addMethod ?? NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.AddMethod (null));
      removeMethod = removeMethod ?? NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.RemoveMethod (null));
      raiseMethod = raiseMethod ?? NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType o) => o.RaiseMethod ("", 7));
      customAttributes = customAttributes ?? new ICustomAttributeData[0];

      return new TestableCustomEventInfo (declaringType, name, attributes, addMethod, removeMethod, raiseMethod)
             {
                 CustomAttributeDatas = customAttributes
             };
    }

    delegate void MyEventDelegate (string arg1, int arg2);

    class DomainType
    {
      public void AddMethod (MyEventDelegate delegate_) { }
      public void RemoveMethod (MyEventDelegate delegate_) { }
      public void RaiseMethod (string arg1, int arg2) { }
    }
  }
}