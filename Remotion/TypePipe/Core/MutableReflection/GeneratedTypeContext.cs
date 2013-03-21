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
using System.Linq;
using System.Reflection;
using Remotion.Collections;
using Remotion.Reflection.MemberSignatures;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// A context that holds the completed types after code generation. This class can be used to retrieve a generated <see cref="MemberInfo"/>
  /// with the corresponding <see cref="IMutableMember"/>.
  /// </summary>
  public class GeneratedTypeContext
  {
    private const BindingFlags c_allDeclared =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private readonly Dictionary<IMutableMember, MemberInfo> _mapping;

    public GeneratedTypeContext (IEnumerable<Tuple<MutableType, Type>> mutableAndGeneratedTypes)
    {
      ArgumentUtility.CheckNotNull ("mutableAndGeneratedTypes", mutableAndGeneratedTypes);

      _mapping = mutableAndGeneratedTypes.ToDictionary (t => (IMutableMember) t.Item1, t => (MemberInfo) t.Item2);
    }
    
    // TODO 5482: docs
    public MemberInfo GetGeneratedMember (IMutableMember mutableMember)
    {
      ArgumentUtility.CheckNotNull ("mutableMember", mutableMember);

      MemberInfo generatedMember;
      if (_mapping.TryGetValue (mutableMember, out generatedMember))
        return generatedMember;

      BuildMapping (mutableMember.MutableDeclaringType);

      return _mapping[mutableMember];
    }

    private void BuildMapping (MutableType mutableType)
    {
      var generatedType = (Type) _mapping[mutableType];
      var generatedMembers = generatedType.GetMembers (c_allDeclared);
      var generatedMembersByNameAndSig = generatedMembers.ToDictionary (m => Tuple.Create (m.Name, MemberSignatureProvider.GetMemberSignature (m)));

      var addedMembers = 
          new IMutableMember[] { mutableType.MutableTypeInitializer }.Where(m => m != null)
          .Concat (mutableType.AddedFields.Cast<IMutableMember>())
          .Concat (mutableType.AddedConstructors.Cast<IMutableMember>())
          .Concat (mutableType.AddedMethods.Cast<IMutableMember>())
          .Concat (mutableType.AddedProperties.Cast<IMutableMember>())
          .Concat (mutableType.AddedEvents.Cast<IMutableMember>());

      foreach (var addedMember in addedMembers)
      {
        var member = (MemberInfo) addedMember;
        var nameAndSig = Tuple.Create(member.Name, MemberSignatureProvider.GetMemberSignature (member));
        var generatedMember = generatedMembersByNameAndSig[nameAndSig];

        _mapping.Add (addedMember, generatedMember);
      }
    }
  }
}