using System;
using System.Collections.Generic;
using Remotion.TypePipe.CodeGeneration;

namespace Remotion.TypePipe.TypeAssembly
{
  /// <summary>
  /// Provides functionality for assembling a type by orchestrating <see cref="ITypeAssemblyParticipant"/> instances and an instance of 
  /// <see cref="ICodeGenerator"/>.
  /// </summary>
  public class TypeAssembler
  {
    public TypeAssembler (IEnumerable<ITypeAssemblyParticipant> typeAssemblyParticipants, ICodeGenerator codeGenerator)
    {
      throw new NotImplementedException();
    }

    public Type AssembleType (Type requestedType)
    {
      throw new NotImplementedException();
    }
  }
}