using System;
using JetBrains.Annotations;

namespace Remotion.TypePipe.TypeAssembly.Implementation
{
  public interface IParticipantState
  {
    void AddAdditionalType ([NotNull] object additionalTypeID, [NotNull] Type additionalType);

    [CanBeNull]
    Type GetAdditionalType ([NotNull] object additionalTypeID);

    void AddState ([NotNull] string id, [NotNull] object value);

    [CanBeNull]
    object GetState (string id);
  }
}