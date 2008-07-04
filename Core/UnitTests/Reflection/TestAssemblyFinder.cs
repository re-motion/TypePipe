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
using System.Collections.Generic;
using System.Reflection;
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection
{
  public class TestAssemblyFinder : AssemblyFinder
  {
    public TestAssemblyFinder (IAssemblyFinderFilter filter, bool considerDynamicDirectory, string baseDirectory, string relativeSearchPath, string dynamicDirectory)
        : base(filter, considerDynamicDirectory, baseDirectory, relativeSearchPath, dynamicDirectory)
    {
    }

    public TestAssemblyFinder (IAssemblyFinderFilter filter, params Assembly[] rootAssemblies)
        : base(filter, rootAssemblies)
    {
    }

    public new IEnumerable<Assembly> FindAssembliesInPath (string searchPath)
    {
      return base.FindAssembliesInPath (searchPath);
    }
  }
}