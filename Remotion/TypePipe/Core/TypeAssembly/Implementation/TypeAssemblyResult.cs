using System;
using System.Collections.Generic;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.TypeAssembly.Implementation
{
  public class TypeAssemblyResult
  {
    // TODO RM-5895: test

    private static readonly IReadOnlyDictionary<object, Type> s_emptyDictionary = new Dictionary<object, Type>().AsReadOnly();

    private readonly Type _type;
    private readonly IReadOnlyDictionary<object, Type> _additionalTypes;

    public TypeAssemblyResult (Type type)
        : this (type, s_emptyDictionary)
    {
    }

    public TypeAssemblyResult (Type type, IReadOnlyDictionary<object, Type> additionalTypes)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("additionalTypes", additionalTypes);

      _type = type;
      _additionalTypes = additionalTypes;
    }

    public Type Type
    {
      get { return _type; }
    }

    public IReadOnlyDictionary<object, Type> AdditionalTypes
    {
      get { return _additionalTypes; }
    }
  }
}