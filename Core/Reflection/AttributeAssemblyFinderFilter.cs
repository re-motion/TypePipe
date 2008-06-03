/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Reflection
{
  [Serializable]
  public class AttributeAssemblyFinderFilter : IAssemblyFinderFilter
  {
    private readonly Type _attributeType;

    public AttributeAssemblyFinderFilter (Type attributeType)
    {
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("attributeType", attributeType, typeof (Attribute));
      _attributeType = attributeType;
    }

    public bool ShouldConsiderAssembly (AssemblyName assemblyName)
    {
      ArgumentUtility.CheckNotNull ("assemblyName", assemblyName);
      return true;
    }

    public bool ShouldIncludeAssembly (Assembly assembly)
    {
      ArgumentUtility.CheckNotNull ("assembly", assembly);
      return assembly.IsDefined (_attributeType, false);
    }
  }
}
