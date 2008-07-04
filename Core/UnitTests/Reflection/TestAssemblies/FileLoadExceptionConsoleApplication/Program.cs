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
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection.TestAssemblies.FileLoadExceptionConsoleApplication
{
  public class Program
  {
    public static int Main (string[] args)
    {
      AssemblyLoader loader = new AssemblyLoader (ApplicationAssemblyFinderFilter.Instance);
      string path = args[0];
      try
      {
        loader.TryLoadAssembly (path);
        return 0;
      }
      catch (Exception ex)
      {
        Console.WriteLine (ex.Message);
        return 1;
      }
    }
  }
}