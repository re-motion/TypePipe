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
using System.IO;
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection.TestAssemblies.FileLoadExceptionConsoleApplication
{
  public class Program
  {
    public static int Main (string[] args)
    {
      string shadowCopying = args[1];

      AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
      setup.ShadowCopyFiles = shadowCopying;

      AppDomain newDomain = AppDomain.CreateDomain ("FileLoadExceptionConsoleApplication_AppDomain", null, setup);
      newDomain.DoCallBack (Callback);
      
      return 99;
    }

    public static void Callback ()
    {
      string path = Environment.GetCommandLineArgs ()[1];
      AssemblyLoader loader = new AssemblyLoader (ApplicationAssemblyFinderFilter.Instance);
      try
      {
        if (loader.TryLoadAssembly (path) == null)
          Environment.Exit (0);
        else
        {
          Console.WriteLine ("Assembly was loaded, but should not be loaded.");
          Environment.Exit (3);
        }
      }
      catch (FileLoadException ex)
      {
        Console.WriteLine (ex.Message);
        Environment.Exit (1);
      }
      catch (Exception ex)
      {
        Console.WriteLine (ex.Message);
        Environment.Exit (2);
      }
    }
  }
}