// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Utilities;

namespace Remotion.Reflection.MemberSignatures
{
  /// <summary>
  /// Represents an event signature and allows signatures to be compared to each other.
  /// </summary>
  public class EventSignature : IEquatable<EventSignature>
  {
    public static EventSignature Create (EventInfo eventInfo)
    {
      ArgumentUtility.CheckNotNull ("eventInfo", eventInfo);
      return new EventSignature (eventInfo.EventHandlerType);
    }

    private readonly Type _eventHandlerType;

    public EventSignature (Type eventHandlerType)
    {
      ArgumentUtility.CheckNotNull ("eventHandlerType", eventHandlerType);
      _eventHandlerType = eventHandlerType;
    }

    public Type EventHandlerType
    {
      get { return _eventHandlerType; }
    }

    public override string ToString ()
    {
      return _eventHandlerType.ToString();
    }

    public virtual bool Equals (EventSignature other)
    {
      return !ReferenceEquals (other, null) 
          && EventHandlerType == other.EventHandlerType;
    }

    public sealed override bool Equals (object obj)
    {
      if (obj == null || obj.GetType() != GetType())
        return false;

      var other = (EventSignature) obj;
      return Equals(other);
    }

    public override int GetHashCode ()
    {
      return EventHandlerType.GetHashCode();
    }
  }
}