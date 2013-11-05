using System;
using System.Collections.Generic;

namespace Remotion.TypePipe.TypeAssembly.Implementation
{
  /// <threadsafety static="true" instance="false"/>
  public class ParticipantState : IParticipantState
  {
    // TODO RM-5895: test

    private readonly Dictionary<object, Type> _additionalTypes = new Dictionary<object, Type>();
    private readonly Dictionary<string, object> _state = new Dictionary<string, object>();

    public ParticipantState ()
    {
    }

    public void AddAdditionalType (object additionalTypeID, Type additionalType)
    {
      _additionalTypes.Add (additionalTypeID, additionalType);
    }

    public Type GetAdditionalType (object additionalTypeID)
    {
      Type additionalType;
      if (_additionalTypes.TryGetValue (additionalTypeID, out additionalType))
        return additionalType;
      return null;
    }

    public void AddState (string id, object value)
    {
      _state.Add (id, value);
    }

    public object GetState (string id)
    {
      object value;
      if (_state.TryGetValue (id, out value))
        return value;
      return null;
    }
  }
}