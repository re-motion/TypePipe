using System;
using System.Collections.Generic;
using Remotion.Utilities;

namespace Remotion.TypePipe.TypeAssembly
{
  ///<summary>Default implementation of the <see cref="IParticipantState"/> interface.</summary>
  /// <threadsafety static="true" instance="false"/>
  public class ParticipantState : IParticipantState
  {
    private readonly Dictionary<string, object> _state = new Dictionary<string, object>();

    public ParticipantState ()
    {
    }

    public void AddState (string id, object value)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("id", id);
      ArgumentUtility.CheckNotNull ("value", value);

      if (_state.ContainsKey (id))
        throw new InvalidOperationException (string.Format ("State identified by the id '{0}' already exists. State identifier must be unique.", id));

      _state.Add (id, value);
    }

    public object GetState (string id)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("id", id);

      object value;
      if (_state.TryGetValue (id, out value))
        return value;
      return null;
    }
  }
}