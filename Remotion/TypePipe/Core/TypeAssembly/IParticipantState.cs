using System;
using JetBrains.Annotations;

namespace Remotion.TypePipe.TypeAssembly
{
  public interface IParticipantState
  {
    void AddState ([NotNull] string id, [NotNull] object value);

    [CanBeNull]
    object GetState (string id);
  }
}