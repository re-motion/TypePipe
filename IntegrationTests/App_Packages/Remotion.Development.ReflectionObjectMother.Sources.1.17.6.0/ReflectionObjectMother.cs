// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.UnitTesting.Reflection
{
  static partial class ReflectionObjectMother
  {
    private static readonly Random s_random = new Random ();

    private static readonly Type[] s_nonGenericTypes = EnsureNoNulls (new[] { typeof (DateTime), typeof (Random), typeof (sbyte), typeof (Uri) });
    private static readonly Type[] s_otherTypes = EnsureNoNulls (new[] { typeof (decimal), typeof (StringBuilder) });
    private static readonly Type[] s_genericTypeDefinition = EnsureNoNulls (new[] { typeof (IComparable<>), typeof (Dictionary<,>) });
    private static readonly Type[] s_genericParameters = EnsureNoNulls (typeof (Dictionary<,>).GetGenericArguments());
    private static readonly Type[] s_otherGenericParameters = EnsureNoNulls (typeof (Lookup<,>).GetGenericArguments());
    private static readonly Type[] s_serializableTypes = EnsureNoNulls (new[] { typeof (object), typeof (string), typeof (List<int>) });
    private static readonly Type[] s_unsealedTypes = EnsureNoNulls (new[] { typeof (Exception), typeof (List<int>) });
    private static readonly Type[] s_sealedTypes = EnsureNoNulls (new[] { typeof (string), typeof (ReflectionObjectMother) });
    private static readonly Type[] s_delegateTypes = EnsureNoNulls (new[] { typeof (EventHandler), typeof (Action<,,>) });
    private static readonly Type[] s_valueTypes = EnsureNoNulls (new[] { typeof (double), typeof (DateTime) });
    private static readonly Type[] s_classTypes = EnsureNoNulls (new[] { typeof (StringBuilder), typeof (Exception) });
    private static readonly Type[] s_interfaceTypes = EnsureNoNulls (new[] { typeof (IDisposable), typeof (IServiceProvider) });
    private static readonly Type[] s_otherInterfaceTypes = EnsureNoNulls (new[] { typeof (IComparable), typeof (ICloneable) });
    private static readonly Type[] s_nestedTypes = EnsureNoNulls (new[] { typeof (DomainType), typeof (DomainTypeBase) });
    private static readonly FieldInfo[] s_staticFields = EnsureNoNulls (new[] { typeof (string).GetField ("Empty"), typeof (Type).GetField ("EmptyTypes") });
    private static readonly FieldInfo[] s_instanceFields = EnsureNoNulls (new[] { typeof (DomainType).GetField ("Field") });
    private static readonly ConstructorInfo[] s_staticCtors = EnsureNoNulls (new[] { typeof (DomainType).TypeInitializer });
    private static readonly ConstructorInfo[] s_defaultCtors = EnsureNoNulls (new[] { typeof (object).GetConstructor (Type.EmptyTypes), typeof (List<int>).GetConstructor (Type.EmptyTypes) });
    private static readonly MethodInfo[] s_instanceMethod = EnsureNoNulls (new[] { typeof (object).GetMethod ("ToString"), typeof (object).GetMethod ("GetHashCode") });
    private static readonly MethodInfo[] s_staticMethod = EnsureNoNulls (new[] { typeof (object).GetMethod ("ReferenceEquals"), typeof (double).GetMethod ("IsNaN") });
    private static readonly MethodInfo[] s_virtualMethods = EnsureNoNulls (new[] { typeof (object).GetMethod ("ToString"), typeof (object).GetMethod ("GetHashCode") });
    private static readonly MethodInfo[] s_nonVirtualMethods = EnsureNoNulls (new[] { typeof (object).GetMethod ("ReferenceEquals"), typeof (string).GetMethod ("Concat", new[] { typeof (object) }) });
    private static readonly MethodInfo[] s_nonVirtualInstanceMethods = EnsureNoNulls (new[] { typeof (object).GetMethod ("GetType"), typeof (string).GetMethod ("Contains", new[] { typeof (string) }) });
    private static readonly MethodInfo[] s_overridingMethods = EnsureNoNulls (new[] { typeof (DomainType).GetMethod ("Override"), typeof (DomainType).GetMethod ("ToString") });
    private static readonly MethodInfo[] s_finalMethods = EnsureNoNulls (new[] { typeof (DomainType).GetMethod ("FinalMethod") });
    private static readonly MethodInfo[] s_nonGenericMethods = EnsureNoNulls (new[] { typeof (object).GetMethod ("ToString"), typeof (string).GetMethod ("Concat", new[] { typeof (object) }) });
    private static readonly MethodInfo[] s_genericMethods = EnsureNoNulls (new[] { typeof (Enumerable).GetMethod ("Empty"), typeof (ReflectionObjectMother).GetMethod ("GetRandomElement", BindingFlags.NonPublic | BindingFlags.Static) });
    private static readonly MethodInfo[] s_methodInstantiations = EnsureNoNulls (new[] { typeof (Enumerable).GetMethod ("Empty").MakeGenericMethod(typeof(int)), typeof (ReflectionObjectMother).GetMethod ("GetRandomElement", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(typeof(string)) });
    private static readonly MethodInfo[] s_abstractMethodInfos = EnsureNoNulls (new[] { typeof (MethodInfo).GetMethod ("GetBaseDefinition"), typeof (Type).GetMethod ("GetMethods", new[] { typeof (BindingFlags) }) });
    private static readonly MethodInfo[] s_nonPublicMethodInfos = EnsureNoNulls (new[] { typeof (DomainType).GetMethod ("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance), typeof (DomainType).GetMethod ("ProtectedMethod", BindingFlags.NonPublic | BindingFlags.Instance) });
    private static readonly MethodInfo[] s_publicMethodInfos = EnsureNoNulls (new[] { typeof (DomainType).GetMethod ("FinalMethod"), typeof (DomainType).GetMethod ("Override") });
    private static readonly ParameterInfo[] s_parameterInfos = EnsureNoNulls (typeof (Dictionary<,>).GetMethod ("TryGetValue").GetParameters ());
    private static readonly PropertyInfo[] s_properties = EnsureNoNulls (new[] { typeof (List<>).GetProperty ("Count"), typeof (Type).GetProperty ("IsArray") });
    private static readonly PropertyInfo[] s_staticProperties = EnsureNoNulls (new[] { typeof (Environment).GetProperty ("CurrentDirectory"), typeof (Type).GetProperty ("DefaultBinder") });
    private static readonly EventInfo[] s_events = EnsureNoNulls (new[] { typeof (INotifyPropertyChanged).GetEvent ("PropertyChanged"), typeof (AppDomain).GetEvent ("AssemblyLoad") });
    private static readonly EventInfo[] s_staticEvents = EnsureNoNulls (new[] { typeof (DomainType).GetEvent ("StaticEvent") });

    public static Type GetSomeType ()
    {
      return GetRandomElement (s_nonGenericTypes);
    }

    public static Type GetSomeOtherType ()
    {
      return GetRandomElement (s_otherTypes);
    }

    public static Type GetSomeNonGenericType ()
    {
      var type = GetRandomElement (s_nonGenericTypes);
      Assertion.IsFalse (type.IsGenericType);
      return type;
    }

    public static Type GetSomeGenericTypeDefinition ()
    {
      var genericType = GetRandomElement (s_genericTypeDefinition);
      Assertion.IsTrue (genericType.IsGenericTypeDefinition);
      return genericType;
    }

    public static Type GetSomeGenericParameter ()
    {
      var genericParameter = GetRandomElement (s_genericParameters);
      Assertion.IsTrue (genericParameter.IsGenericParameter);
      return genericParameter;
    }

    public static Type GetSomeOtherGenericParameter ()
    {
      var genericParameter = GetRandomElement (s_otherGenericParameters);
      Assertion.IsTrue (genericParameter.IsGenericParameter);
      return genericParameter;
    }

    public static Type GetSomeSerializableType ()
    {
      var type = GetRandomElement (s_serializableTypes);
      Assertion.IsTrue (type.IsSerializable);
      return type;
    }

    public static Type GetSomeSubclassableType ()
    {
      var type = GetRandomElement (s_unsealedTypes);
      Assertion.IsTrue (type.IsClass);
      Assertion.IsFalse (type.IsSealed);
      return type;
    }

    public static Type GetSomeNonSubclassableType ()
    {
      var type = GetRandomElement (s_sealedTypes);
      Assertion.IsTrue (type.IsClass);
      Assertion.IsTrue (type.IsSealed);
      return type;
    }

    public static Type GetSomeDelegateType ()
    {
      var type = GetRandomElement (s_delegateTypes);
      Assertion.IsTrue (typeof (Delegate).IsAssignableFrom (type));
      return type;
    }

    public static Type GetSomeValueType ()
    {
      var type = GetRandomElement (s_valueTypes);
      Assertion.IsTrue (type.IsValueType);
      return type;
    }

    public static Type GetSomeClassType ()
    {
      var type = GetRandomElement (s_classTypes);
      Assertion.IsTrue (type.IsClass);
      return type;
    }

    public static Type GetSomeInterfaceType ()
    {
      var type = GetRandomElement (s_interfaceTypes);
      Assertion.IsTrue (type.IsInterface);
      return type;
    }

    public static Type GetSomeOtherInterfaceType ()
    {
      var type = GetRandomElement (s_otherInterfaceTypes);
      Assertion.IsTrue (type.IsInterface);
      return type;
    }

    public static Type GetSomeNestedType ()
    {
      var type = GetRandomElement (s_nestedTypes);
      Assertion.IsTrue (type.IsNested);
      return type;
    }

    public static MemberInfo GetSomeMember ()
    {
      return GetRandomElement (
          AllMethods.Cast<MemberInfo> ()
              .Concat (s_instanceFields)
              .Concat (s_staticFields)
              .Concat (s_defaultCtors)
              .ToArray ());
    }

    public static FieldInfo GetSomeField ()
    {
      return GetRandomElement (s_instanceFields);
    }

    public static FieldInfo GetSomeOtherField ()
    {
      return GetRandomElement (s_staticFields);
    }

    public static FieldInfo GetSomeInstanceField ()
    {
      var field = GetRandomElement (s_instanceFields);
      Assertion.IsFalse (field.IsStatic);
      return field;
    }

    public static FieldInfo GetSomeStaticField ()
    {
      var field = GetRandomElement (s_staticFields);
      Assertion.IsTrue (field.IsStatic);
      return field;
    }

    public static ConstructorInfo GetSomeTypeInitializer ()
    {
      var constructor = GetRandomElement (s_staticCtors);
      Assertion.IsTrue (constructor.IsStatic);
      return constructor;
    }

    public static ConstructorInfo GetSomeDefaultConstructor ()
    {
      return GetRandomElement (s_defaultCtors);
    }

    public static ConstructorInfo GetSomeConstructor ()
    {
      return GetSomeDefaultConstructor ();
    }

    public static ConstructorInfo GetSomeOtherConstructor ()
    {
      return GetRandomElement (s_staticCtors);
    }

    public static MethodInfo GetSomeMethod ()
    {
      return GetRandomElement (AllMethods.Except (s_genericMethods).ToArray ());
    }

    public static MethodInfo GetSomeOtherMethod ()
    {
      return GetRandomElement (s_genericMethods);
    }

    public static MethodInfo GetSomeInstanceMethod ()
    {
      var method = GetRandomElement (s_instanceMethod);
      Assertion.IsFalse (method.IsStatic);
      return method;
    }

    public static MethodInfo GetSomeStaticMethod ()
    {
      var method = GetRandomElement (s_staticMethod);
      Assertion.IsTrue (method.IsStatic);
      return method;
    }

    public static MethodInfo GetSomeVirtualMethod ()
    {
      var method = GetRandomElement (s_virtualMethods);
      Assertion.IsTrue (method.IsVirtual);
      return method;
    }

    public static MethodInfo GetSomeNonVirtualMethod ()
    {
      var method = GetRandomElement (s_nonVirtualMethods);
      Assertion.IsFalse (method.IsVirtual);
      return method;
    }

    public static MethodInfo GetSomeNonVirtualInstanceMethod ()
    {
      var method = GetRandomElement (s_nonVirtualInstanceMethods);
      Assertion.IsFalse (method.IsVirtual);
      Assertion.IsFalse (method.IsStatic);
      return method;
    }

    public static MethodInfo GetSomeAbstractMethod ()
    {
      var method = GetRandomElement (s_abstractMethodInfos);
      Assertion.IsTrue (method.IsAbstract);
      return method;
    }

    public static MethodInfo GetSomeConcreteMethod ()
    {
      var method = GetRandomElement (s_instanceMethod);
      Assertion.IsFalse (method.IsAbstract);
      return method;
    }

    public static MethodInfo GetSomeOverridingMethod ()
    {
      var method = GetRandomElement (s_overridingMethods);
      Assertion.IsTrue (method.GetBaseDefinition () != method);
      return method;
    }

    public static MethodInfo GetSomeBaseDefinition ()
    {
      var method = GetRandomElement (s_instanceMethod).GetBaseDefinition ();
      Assertion.IsTrue (method == method.GetBaseDefinition ());
      return method;
    }

    public static MethodInfo GetSomeFinalMethod ()
    {
      var method = GetRandomElement (s_finalMethods);
      Assertion.IsTrue (method.IsFinal);
      return method;
    }

    public static MethodInfo GetSomeNonGenericMethod ()
    {
      var method = GetRandomElement (s_nonGenericMethods);
      Assertion.IsFalse (method.IsGenericMethod);
      return method;
    }

    public static MethodInfo GetSomeNonPublicMethod ()
    {
      var method = GetRandomElement (s_nonPublicMethodInfos);
      Assertion.IsFalse (method.IsPublic);
      return method;
    }

    public static MethodInfo GetSomePublicMethod ()
    {
      var method = GetRandomElement (s_publicMethodInfos);
      Assertion.IsTrue (method.IsPublic);
      return method;
    }

    public static MethodInfo GetSomeGenericMethodDefinition ()
    {
      var method = GetRandomElement (s_genericMethods);
      Assertion.IsTrue (method.IsGenericMethodDefinition);
      return method;
    }

    public static MethodInfo GetSomeMethodInstantiation ()
    {
      var method = GetRandomElement (s_methodInstantiations);
      Assertion.IsTrue (method.IsGenericMethod);
      Assertion.IsFalse (method.IsGenericMethodDefinition);
      return method;
    }

    public static MethodInfo[] GetMultipeMethods (int count)
    {
      var result = s_nonGenericMethods.Take (count).ToArray ();
      Assertion.IsTrue (result.Length == count, "Count must be at most {0} (or add elements to s_methodInfos).", s_nonGenericMethods.Length);
      return result;
    }

    public static ParameterInfo GetSomeParameter ()
    {
      return GetRandomElement (s_parameterInfos);
    }

    public static PropertyInfo GetSomeProperty ()
    {
      return GetRandomElement (s_properties);
    }

    public static PropertyInfo GetSomeOtherProperty ()
    {
      return GetRandomElement (s_staticProperties);
    }

    public static PropertyInfo GetSomeStaticProperty ()
    {
      var property = GetRandomElement (s_staticProperties);
      var getter = property.GetGetMethod ();
      var setter = property.GetSetMethod ();
      Assertion.IsTrue (getter != null && getter.IsStatic || setter != null && setter.IsStatic);
      return property;
    }

    public static EventInfo GetSomeEvent ()
    {
      return GetRandomElement (s_events);
    }

    public static EventInfo GetSomeOtherEvent ()
    {
      return GetRandomElement (s_staticEvents);
    }

    public static EventInfo GetSomeStaticEvent ()
    {
      var @event = GetRandomElement (s_staticEvents);
      Assertion.IsTrue (@event.GetAddMethod().IsStatic);
      return @event;
    }

    public static object GetDefaultValue (Type type)
    {
      return type.IsValueType ? Activator.CreateInstance (type) : null;
    }

    private static IEnumerable<MethodInfo> AllMethods
    {
      get
      {
        return s_instanceMethod
            .Concat (s_staticMethod)
            .Concat (s_virtualMethods)
            .Concat (s_nonVirtualMethods)
            .Concat (s_nonVirtualInstanceMethods)
            .Concat (s_overridingMethods)
            .Concat (s_finalMethods)
            .Concat (s_nonGenericMethods)
            .Concat (s_genericMethods)
            .Concat (s_abstractMethodInfos);
      }
    }

    private static T GetRandomElement<T> (T[] array)
    {
      var index = s_random.Next (array.Length);
      return array[index];
    }

    private static T[] EnsureNoNulls<T> (T[] items) where T : class
    {
      foreach (var item in items)
        Assertion.IsNotNull (item);
      return items;
    }

    private class DomainTypeBase
    {
      public virtual void FinalMethod () { }

      public virtual void Override () { }
    }

    private class DomainType : DomainTypeBase
    {
      static DomainType () { }

      // ReSharper disable UnusedField.Compiler
      public int Field = 0;
      // ReSharper restore UnusedField.Compiler

      public sealed override void FinalMethod () { }

      public override void Override () { }
      public override string ToString () { return ""; }

      private void PrivateMethod () { }
      protected void ProtectedMethod () { }

#pragma warning disable 67
      public static event Action StaticEvent;
#pragma warning restore 67
    }
  }
}