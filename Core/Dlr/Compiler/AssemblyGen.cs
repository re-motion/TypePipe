/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Text;
using System.Threading;
using Remotion.TypePipe.Dlr.Dynamic.Utils;

#if TypePipe
namespace Remotion.TypePipe.Dlr.Ast.Compiler {
#else
namespace System.Linq.Expressions.Compiler {
#endif
    internal sealed class AssemblyGen {
        private static AssemblyGen _assembly;

        private readonly AssemblyBuilder _myAssembly;
        private readonly ModuleBuilder _myModule;

        private int _index;

        private static AssemblyGen Assembly {
            get {
                if (_assembly == null) {
                    Interlocked.CompareExchange(ref _assembly, new AssemblyGen(), null);
                }
                return _assembly;
            }
        }

        private AssemblyGen() {
            var name = new AssemblyName("Snippets");

#if SILVERLIGHT  // AssemblyBuilderAccess.RunAndSave, Environment.CurrentDirectory
            _myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            _myModule = _myAssembly.DefineDynamicModule(name.Name, false);
#else

            // mark the assembly transparent so that it works in partial trust:
            var attributes = new[] { 
                new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0])
            };

            _myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly (name, AssemblyBuilderAccess.Run, attributes);
            _myModule = _myAssembly.DefineDynamicModule (name.Name, false);

            _myAssembly.DefineVersionInfoResource();
#endif
        }

        private TypeBuilder DefineType(string name, Type parent, TypeAttributes attr) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(parent, "parent");

            StringBuilder sb = new StringBuilder(name);

            int index = Interlocked.Increment(ref _index);
            sb.Append("$");
            sb.Append(index);

            // There is a bug in Reflection.Emit that leads to 
            // Unhandled Exception: System.Runtime.InteropServices.COMException (0x80131130): Record not found on lookup.
            // if there is any of the characters []*&+,\ in the type name and a method defined on the type is called.
            sb.Replace('+', '_').Replace('[', '_').Replace(']', '_').Replace('*', '_').Replace('&', '_').Replace(',', '_').Replace('\\', '_');

            name = sb.ToString();

            return _myModule.DefineType(name, attr, parent);
        }

        internal static TypeBuilder DefineDelegateType(string name) {
            return Assembly.DefineType(
                name,
                typeof(MulticastDelegate),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass
            );
        }
    }

    internal static class SymbolGuids {
        internal static readonly Guid DocumentType_Text =
            new Guid(0x5a869d0b, 0x6611, 0x11d3, 0xbd, 0x2a, 0, 0, 0xf8, 8, 0x49, 0xbd);
    }
}

