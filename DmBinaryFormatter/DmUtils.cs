﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using DmBinaryFormatter.Serializers;
using System.IO;
using System.Runtime.Serialization;
using DmBinaryFormatter.Converters;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DmBinaryFormatter
{
    internal static class DmUtils
    {

        public static List<MemberInfo> GetMembersOrderedByParametersForConstructor(List<MemberInfo> members, ParameterInfo[] parameters)
        {
            var membersParams = members.Where(mi => parameters.Any(pi => pi.Name == mi.Name)).OrderBy(m => m.Name + m.DeclaringType.Name);
            var notMembersParams = members.Where(mi => !parameters.Any(pi => pi.Name == mi.Name)).OrderBy(m => m.Name + m.DeclaringType.Name);

            return membersParams.Union(notMembersParams).ToList();
        }

        /// <summary>
        /// Get the Fields and properties from a current type
        /// </summary>
        public static List<MemberInfo> GetMembers(this Type type,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            List<MemberInfo> targetMembers = new List<MemberInfo>();

            targetMembers.AddRange(type.GetFields(flags));

            // Dont add Properties since they are methods
            //targetMembers.AddRange(type.GetProperties(flags));

            List<MemberInfo> distinctMembers = new List<MemberInfo>(targetMembers.Count);

            foreach (IGrouping<string, MemberInfo> groupedMember in targetMembers.GroupBy(m => m.Name))
            {
                int count = groupedMember.Count();
                var member = groupedMember.FirstOrDefault();

                if (member == null)
                    continue;

                distinctMembers.Add(member);
            }

            return distinctMembers.Where(t => !(IsIndexedProperty(t))).OrderBy(m => m.Name + m.DeclaringType.Name).ToList();

        }

        // .Net Standard 1.5 only
        //public static bool CanRead(this MemberInfo member)
        //{
        //    if (member.MemberType == MemberTypes.Field)
        //    {
        //        FieldInfo fi = (FieldInfo)member;
        //        return fi.IsPublic;
        //    }
        //    else if (member.MemberType == MemberTypes.Property)
        //    {
        //        PropertyInfo pi = (PropertyInfo)member;
        //        return pi.CanRead;
        //    }

        //    return false;
        //}

        //public static bool CanWrite(this MemberInfo member)
        //{
        //    if (member.MemberType == MemberTypes.Field)
        //    {
        //        FieldInfo fi = (FieldInfo)member;
        //        return (!fi.IsLiteral && !fi.IsInitOnly && fi.IsPublic);
        //    }
        //    else if (member.MemberType == MemberTypes.Property)
        //    {
        //        PropertyInfo pi = (PropertyInfo)member;
        //        return pi.CanWrite;
        //    }

        //    return false;
        //}

        /// <summary>
        /// Get a member value
        /// </summary>
        public static object GetValue(this MemberInfo member, object obj)
        {

            FieldInfo fi = member as FieldInfo;
            if (fi != null)
                return fi.GetValue(obj);

            PropertyInfo pi = member as PropertyInfo;
            if (pi != null)
                return pi.GetValue(obj);

            // .net standard 1.6 only
            //if (member.MemberType == MemberTypes.Field)
            //{
            //    FieldInfo fi = (FieldInfo)member;
            //    return fi.GetValue(obj);
            //}
            //else if (member.MemberType == MemberTypes.Property)
            //{
            //    PropertyInfo pi = (PropertyInfo)member;
            //    return pi.GetValue(obj);
            //}

            return null;

        }

        /// <summary>
        /// Set an object value
        /// </summary>
        public static void SetValue(this MemberInfo member, object obj, object value)
        {

            FieldInfo fi = member as FieldInfo;
            if (fi != null)
                fi.SetValue(obj, value);

            PropertyInfo pi = member as PropertyInfo;
            if (pi != null)
                pi.SetValue(obj, value);

            //if (member.MemberType == MemberTypes.Field)
            //{
            //    FieldInfo fi = (FieldInfo)member;
            //    fi.SetValue(obj, value);
            //}
            //else if (member.MemberType == MemberTypes.Property)
            //{
            //    PropertyInfo pi = (PropertyInfo)member;
            //    pi.SetValue(obj, value);
            //}

        }

        /// <summary>
        /// Look if a property is an indexer
        /// </summary>
        public static bool IsIndexedProperty(this MemberInfo member)
        {
            PropertyInfo propertyInfo = member as PropertyInfo;

            return propertyInfo != null ? propertyInfo.GetIndexParameters().Length > 0 : false;
        }

        /// <summary>
        /// Get the member Type
        /// </summary>
        public static Type GetMemberType(this MemberInfo member)
        {

            //return member.GetType();

            FieldInfo fi = member as FieldInfo;
            if (fi != null)
                return fi.FieldType;

            PropertyInfo pi = member as PropertyInfo;
            if (pi != null)
                return pi.PropertyType;


            //if (member.MemberType == MemberTypes.Field)
            //{
            //    FieldInfo fi = (FieldInfo)member;
            //    return fi.FieldType;
            //}
            //else if (member.MemberType == MemberTypes.Property)
            //{
            //    PropertyInfo pi = (PropertyInfo)member;
            //    return pi.PropertyType;
            //}

            return null;

        }

        public static bool IsInstantiable(this Type t)
        {
            var ti = t.GetTypeInfo();

            return !(ti.IsInterface || ti.IsAbstract);
        }

        public static bool IsNullable(this Type t)
        {
            if (t.GetTypeInfo().IsValueType)
                return t.IsNullableType();

            return true;
        }

        public static bool IsNullableType(this Type t)
        {
            var ti = t.GetTypeInfo();
            return (ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsPrimitive(this Type t)
        {
            var ti = t.GetTypeInfo();

            // If enum, we have to retrieve the underlying type
            if (t.IsEnum())
                return (Enum.GetUnderlyingType(t)).IsPrimitive();

            // if it's a nullable<T>
            if (t.IsNullableType())
            {
                Type nonNullable = Nullable.GetUnderlyingType(t);

                // Nullable<Enum> ?
                if (nonNullable.GetTypeInfo().IsEnum)
                {
                    Type nullableUnderlyingType = typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(nonNullable));
                    return nullableUnderlyingType.IsPrimitive();
                }

                return nonNullable.IsPrimitive();
            }

            var isPrimitive = ti.IsPrimitive;

            if (isPrimitive)
                return true;

            // for such types like DateTime whe check the IConvertible interface
            return t.IsIConvertible();
        }

        public static bool IsPrimitiveManagedType(this Type valueType)
        {
            if (valueType == typeof(bool))
                return true;
            else if (valueType == typeof(byte))
                return true;
            else if (valueType == typeof(char))
                return true;
            else if (valueType == typeof(double))
                return true;
            else if (valueType == typeof(float))
                return true;
            else if (valueType == typeof(int))
                return true;
            else if (valueType == typeof(long))
                return true;
            else if (valueType == typeof(short))
                return true;
            else if (valueType == typeof(uint))
                return true;
            else if (valueType == typeof(ulong))
                return true;
            else if (valueType == typeof(ushort))
                return true;
            else if (valueType == typeof(byte[]))
                return true;
            else if (valueType == typeof(DateTime))
                return true;
            else if (valueType == typeof(DateTimeOffset))
                return true;
            else if (valueType == typeof(Decimal))
                return true;
            else if (valueType == typeof(Guid))
                return true;
            else if (valueType == typeof(String))
                return true;
            else if (valueType == typeof(SByte))
                return true;
            else if (valueType == typeof(TimeSpan))
                return true;

            return false;

        }

        public static Type GetBaseType(this Type t)
        {
            var objectTypeInfo = t.GetTypeInfo();

            if (t.IsEnum())
                return (Enum.GetUnderlyingType(t)).GetBaseType();

            if (!objectTypeInfo.IsGenericType)
                return t;

            if (t.IsDictionary())
                return t;

            if (t.IsEnumerable())
                return t;

            var gga = objectTypeInfo.GenericTypeArguments;  // GetGenericArguments();
            if (gga == null || gga.Length <= 0)
                throw new ArgumentException("type not valid");

            return gga[0];
        }

        public static bool IsEnumerable(this Type type)
        {
            if (type.IsArray)
                return true;

            if (typeof(IEnumerable).IsAssignableFrom(type))
                return true;

            return false;
        }

        public static bool IsEnum(this Type type)
        {

            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsISerializable(this Type t)
        {
            if (typeof(ISerializable).IsAssignableFrom(t))
                return true;

            TypeInfo typeInfo = t.GetTypeInfo();

            if (typeInfo.ImplementedInterfaces == null || typeInfo.ImplementedInterfaces.Count() == 0)
                return false;

            foreach (var interf in typeInfo.ImplementedInterfaces)
            {
                if (interf == typeof(ISerializable))
                    return true;
            }

            return false;
        }

        public static bool IsIConvertible(this Type t)
        {
            var isAssignable = typeof(IConvertible).IsAssignableFrom(t);

            if (isAssignable)
                return true;

            if (t.IsNullableType())
            {
                var ut = Nullable.GetUnderlyingType(t);
                isAssignable = typeof(IConvertible).IsAssignableFrom(ut);

                return isAssignable;
            }

            return false;
        }

        public static bool IsDictionary(this Type type)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
                return true;

            TypeInfo typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return true;

            return false;
        }

        public static TypeConverter GetConverter(this Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);

            // Every object could use a TypeConverter, so we exclude it
            if (converter != null && converter.GetType() != typeof(TypeConverter) && converter.CanConvertTo(typeof(string)))
                return converter;

            return null;
        }

        public static object CreateInstance(this Type type, object[] parameters)
        {
            if (type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(type);

            ConstructorInfo constructorInfo = GetParameterizedConstructor(type, true);

            if (constructorInfo == null)
                return null;

            return constructorInfo.Invoke(parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CreateInstance(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(type);

            object instance;

            ConstructorInfo constructorInfo = GetDefaultConstructor(type, true);

            if (constructorInfo == null)
            {
                // try to create an uninitialized object when we have a parameters constructor
                // since we don't know what kind of parameters values we need to pass, we can try this method
                // Exception : Doesn't work if the base class is abstract
                Type rh = typeof(RuntimeHelpers);
                var mi = rh.GetRuntimeMethod("GetUninitializedObject", new Type[] { typeof(Type) });
                instance = mi.Invoke(null, new Object[] { type });
            }
            else
            {
                instance = constructorInfo.Invoke(null);
            }

            return instance;
        }

        public static ParameterInfo[] GetConstructorParameters(this Type type, bool nonPublic)
        {

            ConstructorInfo ci = GetDefaultConstructor(type, nonPublic);

            if (ci != null)
                return null;


            ci = GetParameterizedConstructor(type, nonPublic);

            if (ci == null)
                return null;

            return ci.GetParameters();

        }


        public static ConstructorInfo GetDefaultConstructor(this Type t, bool nonPublic)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if (nonPublic)
                bindingFlags = bindingFlags | BindingFlags.NonPublic;

            var gtcs = t.GetConstructors(bindingFlags);

            return gtcs.SingleOrDefault(c => !c.GetParameters().Any());
        }

        public static ConstructorInfo GetParameterizedConstructor(this Type objectType, bool nonPublic)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if (nonPublic)
                bindingFlags = bindingFlags | BindingFlags.NonPublic;

            IList<ConstructorInfo> constructors = objectType.GetConstructors(bindingFlags).ToList();

            return constructors.Count == 1 ? constructors[0] : null;
        }

        public static ConstructorInfo GetISerializableConstructor(this Type type)
        {
            return type.GetConstructor(new[] { typeof(SerializationInfo), typeof(StreamingContext) });
        }


        public static void WriteLineIndent(this TextWriter writer, int indent)
        {
            writer.WriteLine();
            WriteIndent(writer, indent);

        }
        public static void WriteIndent(this TextWriter writer, int indent)
        {
            for (int i = 0; i < indent; i++)
                writer.Write("  ");

        }

        public static void WriteBytes(this TextWriter writer, byte[] bytes)
        {
            Console.Write("[");
            foreach (var b in bytes)
                Console.Write(b);
            Console.Write("]");
        }

        public static void WriteBytes(this TextWriter writer, byte b)
        {
            Console.Write("[");
            Console.Write(b);
            Console.Write("]");
        }

    }
}
