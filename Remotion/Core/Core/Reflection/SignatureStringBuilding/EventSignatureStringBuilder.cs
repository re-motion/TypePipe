// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Reflection;
using System.Text;
using Remotion.Utilities;

namespace Remotion.Reflection.SignatureStringBuilding
{
  /// <summary>
  /// Builds a string representing the signature of a given <see cref="EventInfo"/> object. This is similar to the string returned by 
  /// <see cref="object.ToString"/> as it contains a textual representation of the event's type.
  /// It's different from <see cref="object.ToString"/>, though, because it does not contain the event's name.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The signature string does currently not hold custom modifiers.
  /// </para>
  /// <para>
  /// For simplicity, this class assumes that type or namespace names cannot contain the character "[". 
  /// It also assumes that the full name of a type (namespace, enclosing type (if any), 
  /// and simple type name) is enough to identify a type - assembly information is not encoded. The 1:1 mapping of signature strings to event 
  /// signatures is only guaranteed for events that adhere to these assumptions.
  /// </para>
  /// </remarks>
  public class EventSignatureStringBuilder : IMemberSignatureStringBuilder
  {
    private readonly MemberSignatureStringBuilderHelper _helper = new MemberSignatureStringBuilderHelper ();

    public string BuildSignatureString (EventInfo eventInfo)
    {
      ArgumentUtility.CheckNotNull ("eventInfo", eventInfo);

      var sb = new StringBuilder();
      _helper.AppendTypeString (sb, eventInfo.EventHandlerType);
      return sb.ToString ();
    }

    string IMemberSignatureStringBuilder.BuildSignatureString (MemberInfo memberInfo)
    {
      return BuildSignatureString ((EventInfo) memberInfo);
    }
  }
}