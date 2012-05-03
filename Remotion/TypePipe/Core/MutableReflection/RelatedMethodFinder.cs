// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.Reflection.MemberSignatures;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Provides useful methods for investigating method overrides.
  /// This is used by <see cref="MutableType"/>.
  /// </summary>
  public class RelatedMethodFinder : IRelatedMethodFinder
  {
    public MethodInfo FindFirstOverriddenMethod (string name, MethodSignature signature, IEnumerable<MethodInfo> candidates)
    {
      return candidates
        .Where (m => m.IsVirtual && m.Name == name && MethodSignature.Create (m).Equals (signature))
        .FirstOrDefault();
    }
  }
}