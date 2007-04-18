using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Utilities;
using Rubicon.Collections;

namespace Rubicon.Reflection
{
  /// <summary>
  /// Use this base class to implement methods that use reflection-like argument type lists, like <see cref="Type.GetMethod (string, Type[])"/>.
  /// </summary>
  /// <remarks>
  /// <![CDATA[
  /// In order to call a specific method or constructor using reflection, you often have to provide a list of argument types. However, manually 
  /// specifying this list is tedious and error-prone. Consider the following example (assuming Window is derived from Control):
  /// 
  ///   void SampleMethod (int i, string s, object o);
  ///   void SampleMethod (int i, string s, Control c);
  ///   void SampleMethod (int i, string s, Window w);
  ///    
  ///   int integer; string str; Control ctrl;
  ///   MethodInfo method = type.GetMethod ("SampleMethod", new Type[] { typeof(int), typeof(string), typeof(Control) });
  ///   method.Invoke (new object[] { integer, str, ctrl });
  /// 
  /// A common workaround is to accept an array of values and determine the types from those values via GetType(), like Activator.CreateInstance()
  /// does. In fact, this is just what InvokeWith.Invoke (object[]) does. 
  /// The problem with this type of overload is that the types are determined at runtime. In the previous example, different methods would be called
  /// depending on the runtime value of ctrl. If ctrl is null, the first overload would be called (because the runtime type of null can only be resolved
  /// to object). If ctrl is a Window, the third overload would be called. In any other case, the second overload would be called. This is usually
  /// not what programmers would expect.
  /// If ctrl is of a type for which no explicit overload exists, the overload for type object is called (overload no. 1). If there is no overload for
  /// the object type, the call usually results in an exception.
  /// 
  /// InvokeWith uses generic methods to pass the compile-time type information to the invoke method.
  /// 
  ///   invoker.CallMethod (obj, "SampleMethod").With (integer, str, ctrl);
  /// 
  /// would always call the second overload of SampleMethod, because the variable ctrl has the static type Control. The previous line is the same as 
  /// the more explicit
  /// 
  ///   invoker.CallMethod (obj, "SampleMethod").With&lt;int,string,Control&gt; (integer, str, ctrl);
  /// 
  /// The following table gives an overview on which overloads would be selected given a value of ctrl:
  /// 
  /// Value of ctrl         Dynamic (get types from object[])   InvokeWith.With<A1,A2,A3>
  /// ---------------------------------------------------------------------------------------------
  /// null                  SampleMethod (int, string, object)  SampleMethod (int, string, Control)
  /// new Control()         SampleMethod (int, string, Control) SampleMethod (int, string, Control)
  /// new Window()          SampleMethod (int, string, Window)  SampleMethod (int, string, Control)
  /// new SpecialControl()  no match                            SampleMethod (int, string, Control)
  /// ]]> 
  /// To use InvokeWith, create a method that returns an instance of a subclass of InvokeWith. Pass your method's parameters to this instance and
  /// do the actual processing in your override of Invoke(). Refer to <see ref="TypesafeActivator"/> for an example implementation of a method 
  /// using InvokeWith.
  /// </remarks>
  /// <typeparam name="T"> Return type of yourMethod(...).With(...)</typeparam>
  public struct InvokeWith<T> : IInvokeWith<T>
  {
    private GetDelegateWith<T> _getDelegateWith;

    public InvokeWith (GetDelegateWith<T> getDelegateWith)
    {
      _getDelegateWith = getDelegateWith;
    }

    public T With (Type[] valueTypes, object[] values)
    {
      ArgumentUtility.CheckNotNull ("valueTypes", valueTypes);
      ArgumentUtility.CheckNotNull ("values", values);
      if (valueTypes.Length != values.Length)
        throw new InvalidOperationException ("Arguments must be of same size.");

      #if DEBUG
        for (int i = 0; i < values.Length; ++i)
          Assertion.Assert (values[i] == null || valueTypes[i].IsAssignableFrom (values[i].GetType ()), "Incompatible types in InvokeWith.With() at array index " + i.ToString () + ".");
      #endif

      return (T) _getDelegateWith.With (valueTypes).DynamicInvoke (values);
    }

    public T With (object[] values)
    {
      ArgumentUtility.CheckNotNull ("values", values);

      Type[] valueTypes = new Type[values.Length];
      for (int i = 0; i < values.Length; ++i)
      {
        object value = values[i];
        valueTypes[i] = (value != null) ? value.GetType () : typeof (object);
      }

      return (T) _getDelegateWith.With (valueTypes).DynamicInvoke (values);
    }

    public T With ()
    {
      return _getDelegateWith.With () ();
    }
    public T With<A1> (A1 a1)
    {
      return _getDelegateWith.With<A1> () (a1);
    }
    public T With<A1, A2> (A1 a1, A2 a2)
    {
      return _getDelegateWith.With<A1, A2> () (a1, a2);
    }
    public T With<A1, A2, A3> (A1 a1, A2 a2, A3 a3)
    {
      return _getDelegateWith.With<A1, A2, A3> () (a1, a2, a3);
    }
    public T With<A1, A2, A3, A4> (A1 a1, A2 a2, A3 a3, A4 a4)
    {
      return _getDelegateWith.With<A1, A2, A3, A4> () (a1, a2, a3, a4);
    }
    public T With<A1, A2, A3, A4, A5> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
    {
      return _getDelegateWith.With<A1, A2, A3, A4, A5> () (a1, a2, a3, a4, a5);
    }
    public T With<A1, A2, A3, A4, A5, A6> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6)
    {
      return _getDelegateWith.With<A1, A2, A3, A4, A5, A6> () (a1, a2, a3, a4, a5, a6);
    }
    public T With<A1, A2, A3, A4, A5, A6, A7> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7)
    {
      return _getDelegateWith.With<A1, A2, A3, A4, A5, A6, A7> () (a1, a2, a3, a4, a5, a6, a7);
    }
  }  

//  public abstract class InvokeWithObsolete<T>
//  {
//    /// <summary>
//    /// Implement this method to perform the actual invocation. 
//    /// </summary>
//    /// <param name="valueTypes"> The value types as determined by the calling method. </param>
//    /// <param name="values"> The actual values. </param>
//    protected abstract T Invoke (Type[] valueTypes, object[] values);

//    public T With (Type[] valueTypes, object[] values)
//    {
//      ArgumentUtility.CheckNotNull ("valueTypes", valueTypes);
//      ArgumentUtility.CheckNotNull ("values", values);
//      if (valueTypes.Length != values.Length)
//        throw new InvalidOperationException ("Arguments must be of same size.");

//#if DEBUG
//      for (int i = 0; i < values.Length; ++i)
//        Assertion.Assert (values[i] == null || valueTypes[i].IsAssignableFrom (values[i].GetType ()), "Incompatible types in InvokeWith.With() at array index " + i.ToString () + ".");
//#endif

//      return Invoke (valueTypes, values);
//    }

//    public T With (object[] values)
//    {
//      ArgumentUtility.CheckNotNull ("values", values);

//      Type[] valueTypes = new Type[values.Length];
//      for (int i = 0; i < values.Length; ++i)
//      {
//        object value = values[i];
//        valueTypes[i] = (value != null) ? value.GetType () : typeof (object);
//      }
//      return Invoke (valueTypes, values);
//    }

//    public T With ()
//    {
//      return Invoke (Type.EmptyTypes, new object[0]);
//    }
//    public T With<A1> (A1 a1)
//    {
//      return Invoke (new Type[] { typeof (A1) }, new object[] { a1 });
//    }
//    public T With<A1, A2> (A1 a1, A2 a2)
//    {
//      return Invoke (new Type[] { typeof (A1), typeof (A2) }, new object[] { a1, a2 });
//    }
//    public T With<A1, A2, A3> (A1 a1, A2 a2, A3 a3)
//    {
//      return Invoke (new Type[] { typeof (A1), typeof (A2), typeof (A3) }, new object[] { a1, a2, a3 });
//    }
//    public T With<A1, A2, A3, A4> (A1 a1, A2 a2, A3 a3, A4 a4)
//    {
//      return Invoke (new Type[] { typeof (A1), typeof (A2), typeof (A3), typeof (A4) }, new object[] { a1, a2, a3, a4 });
//    }
//    public T With<A1, A2, A3, A4, A5> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
//    {
//      return Invoke (new Type[] { typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5) }, new object[] { a1, a2, a3, a4, a5 });
//    }
//    public T With<A1, A2, A3, A4, A5, A6> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6)
//    {
//      return Invoke (new Type[] { typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6) }, new object[] { a1, a2, a3, a4, a5, a6 });
//    }
//    public T With<A1, A2, A3, A4, A5, A6, A7> (A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7)
//    {
//      return Invoke (new Type[] { typeof (A1), typeof (A2), typeof (A3), typeof (A4), typeof (A5), typeof (A6), typeof (A7) }, new object[] { a1, a2, a3, a4, a5, a6, a7 });
//    }
//  }
}
