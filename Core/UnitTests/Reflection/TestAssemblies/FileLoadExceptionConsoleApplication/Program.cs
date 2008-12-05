// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
//
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
//
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
//
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
