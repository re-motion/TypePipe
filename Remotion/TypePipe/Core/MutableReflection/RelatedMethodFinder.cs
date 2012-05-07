// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq;
using System.Reflection;
using Remotion.FunctionalProgramming;
using Remotion.Reflection.MemberSignatures;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Provides useful methods for investigating method overrides.
  /// This is used by <see cref="MutableType"/>.
  /// </summary>
  public class RelatedMethodFinder : IRelatedMethodFinder
  {
    public MethodInfo GetBaseMethod (string name, MethodSignature signature, Type typeToStartSearch)
    {
      var baseTypeSequence = typeToStartSearch.CreateSequence (t => t.BaseType);

      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
      var allBaseMethods = baseTypeSequence.SelectMany (t => t.GetMethods (bindingFlags));

      return allBaseMethods
          .Where (m => m.IsVirtual && m.Name == name && MethodSignature.Create (m).Equals (signature))
          .FirstOrDefault();
    }
  }
}