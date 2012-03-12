using System;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  // TODO Docs
  public class FutureTypeTemplate : ITypeTemplate
  {
    private readonly Type _baseType;
    private readonly TypeAttributes _attributes;
    private readonly Type[] _interfaces;
    private readonly FieldInfo[] _fields;
    private readonly ConstructorInfo[] _constructors;

    public FutureTypeTemplate (Type baseType, TypeAttributes attributes, Type[] interfaces, FieldInfo[] fields, ConstructorInfo[] constructors)
    {
      ArgumentUtility.CheckNotNull ("baseType", baseType);
      ArgumentUtility.CheckNotNull ("interfaces", interfaces);
      ArgumentUtility.CheckNotNull ("fields", fields);
      ArgumentUtility.CheckNotNull ("constructors", constructors);

      _baseType = baseType;
      _attributes = attributes;
      _interfaces = interfaces;
      _fields = fields;
      _constructors = constructors;
    }

    public Type GetBaseType ()
    {
      return _baseType;
    }

    public TypeAttributes GetAttributeFlags ()
    {
      return _attributes;
    }

    public Type[] GetInterfaces ()
    {
      return _interfaces;
    }

    public FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      return _fields;
    }

    public ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      return _constructors;
    }
  }
}