using System;
using System.Collections.Generic;
using Remotion.TypePipe.TypeAssembly.Implementation;

namespace Remotion.TypePipe.TypeAssembly
{
  /// <threadsafety static="true" instance="false"/>
  public class ParticipantState : IParticipantState
  {
    // TODO RM-5895: test

    private readonly Dictionary<string, object> _state = new Dictionary<string, object>();

    public ParticipantState ()
    {
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