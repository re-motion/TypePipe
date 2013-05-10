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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.TypeAssembly
{
  /// <summary>
  /// A context that holds the completed types after code generation. This class can be used to retrieve a generated <see cref="MemberInfo"/>
  /// with the corresponding <see cref="IMutableMember"/>.
  /// </summary>
  public class GeneratedTypesContext
  {
    private const BindingFlags c_allDeclared =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private readonly Dictionary<IMutableMember, MemberInfo> _mapping;

    public GeneratedTypesContext (IEnumerable<KeyValuePair<MutableType, Type>> mutableToGeneratedTypeMapping)
    {
      ArgumentUtility.CheckNotNull ("mutableToGeneratedTypeMapping", mutableToGeneratedTypeMapping);

      _mapping = mutableToGeneratedTypeMapping.ToDictionary (t => (IMutableMember) t.Key, t => (MemberInfo) t.Value);
    }
    
    /// <summary>
    /// Retrieves the generated <see cref="MemberInfo"/> for the specified <see cref="IMutableMember"/>.
    /// </summary>
    /// <param name="mutableMember">The mutable member.</param>
    /// <returns>The generated member.</returns>
    public MemberInfo GetGeneratedMember (IMutableMember mutableMember)
    {
      ArgumentUtility.CheckNotNull ("mutableMember", mutableMember);

      MemberInfo generatedMember;
      if (_mapping.TryGetValue (mutableMember, out generatedMember))
        return generatedMember;

      BuildMapping (mutableMember.MutableDeclaringType);

      return _mapping[mutableMember];
    }

    public Type GetGeneratedType (MutableType mutableType) { return (Type) GetGeneratedMember (mutableType); }
    public Type GetGeneratedNestedType (MutableType mutableType) { return (Type) GetGeneratedMember (mutableType); }
    public FieldInfo GetGeneratedField (MutableFieldInfo mutableField) { return (FieldInfo) GetGeneratedMember (mutableField); }
    public ConstructorInfo GetGeneratedConstructor (MutableConstructorInfo mutableConstructor) { return (ConstructorInfo) GetGeneratedMember (mutableConstructor); }
    public MethodInfo GetGeneratedMethod (MutableMethodInfo mutableMethod) { return (MethodInfo) GetGeneratedMember (mutableMethod); }
    public PropertyInfo GetGeneratedProperty (MutablePropertyInfo mutableProperty) { return (PropertyInfo) GetGeneratedMember (mutableProperty); }
    public EventInfo GetGeneratedEvent (MutableEventInfo mutableEvent) { return (EventInfo) GetGeneratedMember (mutableEvent); }

    private void BuildMapping (MutableType mutableType)
    {
      var generatedType = (Type) _mapping[mutableType];
      var generatedMembers = generatedType.GetMembers (c_allDeclared);
      var generatedMembersByNameAndSig = generatedMembers.ToDictionary (m => Tuple.Create (m.Name, MemberSignatureProvider.GetMemberSignature (m)));

      var addedMembers = 
          new IMutableMember[] { mutableType.MutableTypeInitializer }.Where(m => m != null)
          .Concat (mutableType.AddedNestedTypes.Cast<IMutableMember>())
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