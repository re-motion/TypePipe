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
using System.Reflection;

namespace Remotion.TypePipe.MutableReflection
{
  public class BindingFlagsEvaluator : IBindingFlagsEvaluator
  {
    public bool HasRightAttributes (MethodAttributes methodAttributes, BindingFlags bindingFlags)
    {
      return HasRightVisibility (methodAttributes, bindingFlags) && HasRightInstanceOrStaticFlag (methodAttributes, bindingFlags);
    }

    public bool HasRightAttributes (FieldAttributes fieldAttributes, BindingFlags bindingFlags)
    {
      return HasRightAttributes ((MethodAttributes) fieldAttributes, bindingFlags);
    }

    public bool HasRightVisibility (MethodAttributes methodAttributes, BindingFlags bindingFlags)
    {
      var isPublic = ((methodAttributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public);
      var shoudBePublic = (bindingFlags & BindingFlags.Public) == BindingFlags.Public;

      return isPublic == shoudBePublic;
    }

    public bool HasRightInstanceOrStaticFlag (MethodAttributes methodAttributes, BindingFlags bindingFlags)
    {
      if ((bindingFlags & (BindingFlags.Instance | BindingFlags.Static)) == 0)
        return false;

      var isStatic = (methodAttributes & MethodAttributes.Static) == MethodAttributes.Static;
      var shouldBeStatic = (bindingFlags & BindingFlags.Static) == BindingFlags.Static;

      return isStatic == shouldBeStatic;
    }
  }
}