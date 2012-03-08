using System;
using System.Collections.Generic;
using Remotion.TypePipe.CodeGeneration;
using Remotion.Utilities;

namespace Remotion.TypePipe.TypeAssembly
{
  /// <summary>
  /// Provides functionality for assembling a type by orchestrating <see cref="ITypeAssemblyParticipant"/> instances and an instance of 
  /// <see cref="ITypeModifier"/>.
  /// </summary>
  public class TypeAssembler
  {
    private readonly IEnumerable<ITypeAssemblyParticipant> _participants;
    private readonly ITypeModifier _typeModifier;

    public TypeAssembler (IEnumerable<ITypeAssemblyParticipant> participants, ITypeModifier typeModifier)
    {
      ArgumentUtility.CheckNotNull ("participants", participants);
      ArgumentUtility.CheckNotNull ("typeModifier", typeModifier);

      _participants = participants;
      _typeModifier = typeModifier;
    }

    public IEnumerable<ITypeAssemblyParticipant> Participants
    {
      get { return _participants; }
    }

    public ITypeModifier TypeModifier
    {
      get { return _typeModifier; }
    }

    public Type AssembleType (Type requestedType)
    {
      var modifiedType = _typeModifier.CreateMutableType (requestedType);
      foreach (var participant in _participants)
        participant.ModifyType (modifiedType);

      return _typeModifier.ApplyModifications (modifiedType);
    }
  }
}