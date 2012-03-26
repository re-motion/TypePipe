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
using Remotion.Reflection;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableTypeObjectMother
  {
    public static MutableType Create (
      IUnderlyingTypeStrategy underlyingTypeStrategy = null,
      IEqualityComparer<MemberInfo> memberInfoEqualityComparer = null,
      IBindingFlagsEvaluator bindingFlagsEvaluator = null)
    {
      return new MutableType(
        underlyingTypeStrategy ?? NewTypeStrategyObjectMother.Create(),
        memberInfoEqualityComparer ?? new MemberSignatureEqualityComparer(),
        bindingFlagsEvaluator ?? new BindingFlagsEvaluator());
    }

    public static MutableType CreateStrictMock (IUnderlyingTypeStrategy underlyingTypeStrategy = null,
      IEqualityComparer<MemberInfo> memberInfoEqualityComparer = null,
      IBindingFlagsEvaluator bindingFlagsEvaluator = null)
    {
      return MockRepository.GenerateStrictMock<MutableType> (
        underlyingTypeStrategy ?? NewTypeStrategyObjectMother.Create (),
        memberInfoEqualityComparer ?? new MemberSignatureEqualityComparer (),
        bindingFlagsEvaluator ?? new BindingFlagsEvaluator ());
    }

    public static MutableType CreateForExistingType (Type originalType = null)
    {
      return Create (underlyingTypeStrategy: ExistingTypeStrategyObjectMother.Create (originalType));
    }
  }
}