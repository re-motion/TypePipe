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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  // TODO 4784 <see cref="Item(TMemberInfo)"/>
  /// <summary>
  /// A container storing mutable members and providing convenience properties for <see cref="ExistingMembers"/> and <see cref="AddedMembers"/>.
  /// The indexer xxxx can be used to retrieve the mutable version for an existing member.
  /// </summary>
  /// <typeparam name="TMemberInfo">The type of the existing member infos.</typeparam>
  /// <typeparam name="TMutableMemberInfo">The type of the mutable member infos.</typeparam>
  public class MutableMemberCollection<TMemberInfo, TMutableMemberInfo> : IEnumerable<TMutableMemberInfo>
    where TMemberInfo : MemberInfo
    where TMutableMemberInfo : TMemberInfo
  {
    private readonly MutableType _declaringType;
    private readonly ReadOnlyDictionary<TMemberInfo, TMutableMemberInfo> _existingMembers;
    private readonly List<TMutableMemberInfo> _addedMembers = new List<TMutableMemberInfo>();

    public MutableMemberCollection (
        MutableType declaringType,
        IEnumerable<TMemberInfo> existingMembers,
        Func<TMemberInfo, TMutableMemberInfo> mutableMemberProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("existingMembers", existingMembers);
      ArgumentUtility.CheckNotNull ("mutableMemberProvider", mutableMemberProvider);

      _declaringType = declaringType;
      _existingMembers = existingMembers.ToDictionary (member => member, mutableMemberProvider).AsReadOnly ();
    }

    public ReadOnlyCollectionDecorator<TMutableMemberInfo> ExistingMembers
    {
      get { return _existingMembers.Values.AsReadOnly(); }
    }

    public ReadOnlyCollection<TMutableMemberInfo> AddedMembers
    {
      get { return _addedMembers.AsReadOnly(); }
    }

    public IEnumerator<TMutableMemberInfo> GetEnumerator ()
    {
      return ExistingMembers.Concat (AddedMembers).GetEnumerator ();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator ();
    }

    public TMutableMemberInfo this [TMemberInfo existingMember]
    {
      get
      {
        ArgumentUtility.CheckNotNull ("existingMember", existingMember);
        CheckDeclaringType ("existingMember", existingMember);

        if (existingMember is TMutableMemberInfo)
          return (TMutableMemberInfo) existingMember;

        var mutableMember = _existingMembers.GetValueOrDefault (existingMember);
        if (mutableMember == null)
        {
          var message = string.Format ("The given {0} cannot be modified.", GetMemberTypeName());
          throw new NotSupportedException (message);
        }

        return mutableMember;
      }
    }

    public void AddMember (TMutableMemberInfo mutableMember)
    {
      ArgumentUtility.CheckNotNull ("mutableMember", mutableMember);
      CheckDeclaringType ("mutableMember", mutableMember);

      _addedMembers.Add (mutableMember);
    }

    private void CheckDeclaringType (string parameterName, MemberInfo member)
    {
      if (!_declaringType.IsEquivalentTo (member.DeclaringType))
      {
        var message = string.Format ("{0} is declared by a different type: '{1}'.", GetMemberTypeName(), member.DeclaringType);
        throw new ArgumentException (message, parameterName);
      }
    }

    private string GetMemberTypeName ()
    {
      return typeof (TMemberInfo).Name;
    }
  }
}