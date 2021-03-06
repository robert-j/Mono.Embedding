﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using IKVM.Reflection;
using Mono.Options;

namespace Mono.Embedding.ThunkTool
{
    class Report
    {
        public static void Error(string fmt, params object[] args)
        {
            string msg;

            if (args.Length != 0)
                msg = string.Format(fmt, args);
            else
                msg = fmt;
            Console.WriteLine("error: " + msg);
            Environment.Exit(1);
        }
    }

    class TypeManager
    {
        public static Universe universe = new Universe(UniverseOptions.EnableFunctionPointers | /*UniverseOptions.ResolveMissingMembers*/ UniverseOptions.DisablePseudoCustomAttributeRetrieval);

        public static Assembly mscorlib;
        public static IKVM.Reflection.Type AttributeType;
        public static IKVM.Reflection.Type VoidType, IntPtrType, UIntPtrType, StringType, ObjectType, TypeType, MethodInfoType;
        public static IKVM.Reflection.Type FieldInfoType, PropertyInfoType, AssemblyType, ModuleType;

        public static void Init(string baseCorlibDir)
        {
            if (baseCorlibDir == null)
                baseCorlibDir = typeof(int).Assembly.Location;
            mscorlib = universe.LoadFile(baseCorlibDir);
            if (mscorlib == null)
            {
                Report.Error($"Could not load mscorlib from {baseCorlibDir}");
            }
            AttributeType = mscorlib.GetType("System.Attribute");
            VoidType = mscorlib.GetType("System.Void");
            IntPtrType = mscorlib.GetType("System.IntPtr");
            UIntPtrType = mscorlib.GetType("System.UIntPtr");
            StringType = mscorlib.GetType("System.String");
            ObjectType = mscorlib.GetType("System.Object");
            TypeType = mscorlib.GetType("System.Type");

            MethodInfoType = mscorlib.GetType("System.Reflection.MethodInfo");
            FieldInfoType = mscorlib.GetType("System.Reflection.FieldInfo");
            PropertyInfoType = mscorlib.GetType("System.Reflection.PropertyInfo");
            AssemblyType = mscorlib.GetType("System.Reflection.Assembly");
            ModuleType = mscorlib.GetType("System.Reflection.Module");
        }

    }

    class Program
    {
        struct ThunkMethodInfo
        {
            public readonly string ClrMethodName;
            public readonly string QualMethodName;
            public readonly string QualMethodDecl;
            public readonly string Comments;
            public readonly bool IsGeneric;

            public ThunkMethodInfo(string clrMethodName, string qualMethodName, string qualMethodDecl, string comments, bool isGeneric)
            {
                ClrMethodName = clrMethodName;
                QualMethodName = qualMethodName;
                QualMethodDecl = qualMethodDecl;
                Comments = comments;
                IsGeneric = isGeneric;
            }
        }


        // Base directory to resolve assemblies from
        public static Assembly currentAssembly;
        static string baseCorlibDir;
        static List<ThunkMethodInfo> currentMethods;
        static string outputPrefix;
        static string spec;
        static bool wantComments;
        static readonly StringWriter headerWriter = new StringWriter(CultureInfo.InvariantCulture);
        static readonly StringWriter sourceWriter = new StringWriter(CultureInfo.InvariantCulture);
        static OptionSet optionSet;

        static void Main(string[] args)
        {
            optionSet = new OptionSet()
            {
                {"o|output=",      "output file prefix for .h and .c files", v => outputPrefix = v},
                {"basecorlibdir=", "Base directory to load mscorlib from", v => baseCorlibDir = v },
                {"c|comments",     "emit comments", v => wantComments = v != null},
                {"h|help",         "Show Help", v => Usage (optionSet) }
            };

            try
            {
                args = optionSet.Parse(args).ToArray();
                if (args.Length == 0)
                    Usage(optionSet);
            }
            catch (OptionException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(1);
            }

            TypeManager.Init(baseCorlibDir);

            try
            {
                Process(TypeManager.universe.LoadFile(args[0]));

                if (outputPrefix != null)
                {
                    File.WriteAllText(outputPrefix + ".h", headerWriter.ToString());
                    File.WriteAllText(outputPrefix + ".c", sourceWriter.ToString());
                }
                else
                {
                    Console.Write(headerWriter.ToString());
                    Console.Write(sourceWriter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }

        static void Usage(OptionSet optionSet)
        {
            var writer = Console.Error;
            writer.WriteLine("thunktool [OPTIONS+] assembly-file");
            if (optionSet != null)
            {
                writer.WriteLine("Options:");
                optionSet.WriteOptionDescriptions(writer);
            }
            Environment.Exit(1);
        }

        static void Process(Assembly asm)
        {
            currentAssembly = asm;
            string shortName = asm.GetName().Name;
            string name = shortName.Replace(".", "_");

            var typeList = new List<IKVM.Reflection.Type>();

            headerWriter.WriteLine("/*");
            headerWriter.WriteLine(" * Automatically generated by thunktool from {0}", shortName);
            headerWriter.WriteLine(" */");
            headerWriter.WriteLine();
            headerWriter.WriteLine("#ifndef __{0}_THUNKTOOL__", name.ToUpperInvariant());
            headerWriter.WriteLine("#define __{0}_THUNKTOOL__", name.ToUpperInvariant());
            headerWriter.WriteLine();
            headerWriter.WriteLine("#include <mono/utils/mono-publib.h>");
            headerWriter.WriteLine("#include <mono/metadata/assembly.h>");
            headerWriter.WriteLine("#include <mono/metadata/class.h>");
            headerWriter.WriteLine("#include <mono/metadata/object.h>");
            headerWriter.WriteLine();
            headerWriter.WriteLine("MONO_BEGIN_DECLS");
            headerWriter.WriteLine();
            headerWriter.WriteLine("#ifdef WIN32");
            headerWriter.WriteLine("#define THUNKCALL __stdcall");
            headerWriter.WriteLine("#else");
            headerWriter.WriteLine("#define THUNKCALL");
            headerWriter.WriteLine("#endif");
            headerWriter.WriteLine();

            sourceWriter.WriteLine("/*");
            sourceWriter.WriteLine(" * Automatically generated by thunktool from {0}", shortName);
            sourceWriter.WriteLine(" */");
            sourceWriter.WriteLine();
            sourceWriter.WriteLine("#include <stdlib.h>");
            sourceWriter.WriteLine("#include <string.h>");
            sourceWriter.WriteLine("#include <assert.h>");
            sourceWriter.WriteLine("#include <mono/jit/jit.h>");
            sourceWriter.WriteLine("#include <mono/metadata/reflection.h>");
            sourceWriter.WriteLine("#include \"{0}.h\"", outputPrefix ?? "thunks");
            sourceWriter.WriteLine();

            foreach (var typeInfo in asm.GetTypes ())
            {
                currentMethods = new List<ThunkMethodInfo>();

                foreach (var ctor in typeInfo.GetConstructors ())
                    Process(ctor);

                foreach (var method in typeInfo.GetMethods ())
                    Process(method);

                foreach (var prop in typeInfo.GetProperties ())
                    Process(prop);

                if (currentMethods.Count == 0)
                   continue;

                headerWriter.WriteLine("MonoClass *{0}__Class;", typeInfo.Name);

                foreach (var m in currentMethods) {
                    headerWriter.WriteLine();

                    if (wantComments)
                        headerWriter.WriteLine("/*\n * {0}\n */", m.Comments);

                    if (!m.IsGeneric)
                        headerWriter.WriteLine("{0}", m.QualMethodDecl);
                    else
                        headerWriter.WriteLine("MonoMethod *{0}__Method;", m.QualMethodName);
                }

                headerWriter.WriteLine();

                sourceWriter.WriteLine("static void\n{0}_Init (MonoClass *klass)", typeInfo.Name);
                sourceWriter.WriteLine("{");
                sourceWriter.WriteLine("\tMonoMethod *method;\n");
                sourceWriter.WriteLine("\tassert (klass && \"could not lookup class '{0}'\");", typeInfo.FullName);
                sourceWriter.WriteLine("\t{0}__Class = klass;", typeInfo.Name);
                foreach (var m in currentMethods)
                {
                    sourceWriter.WriteLine();
                    sourceWriter.WriteLine("\tmethod = mono_class_get_method_from_name (klass, \"{0}\", -1);", m.ClrMethodName);
                    sourceWriter.WriteLine("\tassert (method && \"could not lookup method '{0}.{1}'\");", typeInfo.FullName, m.ClrMethodName);
                    if (!m.IsGeneric)
                        sourceWriter.WriteLine("\t{0} = mono_method_get_unmanaged_thunk (method);", m.QualMethodName);
                    else
                        sourceWriter.WriteLine("\t{0}__Method = method;", m.QualMethodName);
                }
                sourceWriter.WriteLine("}\n");

                typeList.Add(typeInfo);
            }

            if (typeList.Count > 0)
            {
                headerWriter.WriteLine("MonoAssembly *{0}_Assembly;", name);
                headerWriter.WriteLine("MonoImage *{0}_Image;", name);
                headerWriter.WriteLine();
                headerWriter.WriteLine("void\n{0}_Init (MonoAssembly *assembly);", name);
                headerWriter.WriteLine();
                headerWriter.WriteLine("void\n{0}_Exec (void);", name);

                sourceWriter.WriteLine("void\n{0}_Init (MonoAssembly *assembly)", name);
                sourceWriter.WriteLine("{");
                sourceWriter.WriteLine("\tstatic int initialized;");
                sourceWriter.WriteLine("\tMonoImage *image;");
                sourceWriter.WriteLine();
                sourceWriter.WriteLine("\tif (initialized) return;");
                sourceWriter.WriteLine();
                sourceWriter.WriteLine("\tassert (assembly && \"{0}\");", name);
                sourceWriter.WriteLine("\timage = mono_assembly_get_image (assembly);");
                sourceWriter.WriteLine("\t{0}_Assembly = assembly;", name);
                sourceWriter.WriteLine("\t{0}_Image = image;", name);
                sourceWriter.WriteLine();

                foreach (var typeInfo in typeList)
                {
                    sourceWriter.WriteLine("\t{1}_Init (mono_class_from_name (image, \"{0}\", \"{1}\"));",
                        typeInfo.Namespace, typeInfo.Name);
                }
                sourceWriter.WriteLine();
                sourceWriter.WriteLine("\tinitialized = 1;");
                sourceWriter.WriteLine("}");
                sourceWriter.WriteLine();

                sourceWriter.WriteLine("void\n{0}_Exec (void)", name);
                sourceWriter.WriteLine("{");
                sourceWriter.WriteLine("\tstatic int initialized;");
                sourceWriter.WriteLine("\tchar *arg0;");
                sourceWriter.WriteLine();
                sourceWriter.WriteLine("\tif (initialized) return;");
                sourceWriter.WriteLine("\tassert ({0}_Assembly);", name);
                sourceWriter.WriteLine("\targ0 = strdup(mono_image_get_filename ({0}_Image));", name);
                sourceWriter.WriteLine("\tmono_jit_exec (mono_domain_get (), {0}_Assembly, 1, &arg0);", name);
                sourceWriter.WriteLine("\tinitialized = 1;");
                sourceWriter.WriteLine("}");
            }

            headerWriter.WriteLine();
            headerWriter.WriteLine("MONO_END_DECLS");
            headerWriter.WriteLine("#endif");
        }

        static void Process(MethodInfo method)
        {
            foreach (var attr in method.__GetCustomAttributes (TypeManager.AttributeType, false))
                if (attr.AttributeType.FullName == "Mono.Embedding.ThunkAttribute")
                    Process(method, method.ReturnType, attr);
        }

        static void Process(ConstructorInfo ctorInfo)
        {
            foreach (var attr in ctorInfo.__GetCustomAttributes (TypeManager.AttributeType, false))
                if (attr.AttributeType.FullName == "Mono.Embedding.ThunkAttribute")
                    Process(ctorInfo, TypeManager.VoidType, attr);
        }

        static void Process(PropertyInfo propInfo)
        {
            foreach (var attr in propInfo.__GetCustomAttributes (TypeManager.AttributeType, false))
                if (attr.AttributeType.FullName == "Mono.Embedding.ThunkAttribute")
                    foreach (var method in propInfo.GetAccessors())
                        Process(method, method.ReturnType, attr);
        }

        static void Process(MethodBase method, IKVM.Reflection.Type returnType, CustomAttributeData attrData)
        {
            string returnTypeString = returnType == TypeManager.VoidType ? "void" : TypeToC(returnType);

            string unmanagedMethodName = method.Name.Replace(".", "_");

            var namedArguments = attrData.NamedArguments ?? new CustomAttributeNamedArgument[0];
            foreach (var arg in namedArguments)
            {
                if (arg.MemberInfo.Name == "ReturnType")
                    returnTypeString = arg.TypedValue.Value.ToString();
                else if (arg.MemberInfo.Name == "Name")
                    unmanagedMethodName = arg.TypedValue.Value.ToString();
            }

            var paramList = new List<string>();

            if (!method.IsStatic)
            {
                // Add *this* parameter
                paramList.Add(FormatParameter(
                    "MonoObject*",
                    TypeToString(method.DeclaringType),
                    "self"
                    ));
            }

            foreach (var info in method.GetParameters())
            {
                paramList.Add(FormatParameter(
                    TypeToC(info.ParameterType),
                    TypeToString(info.ParameterType),
                    info.Name
                    ));
            }
            
            paramList.Add("MonoObject **ex");


            string qualMethodName = String.Format("{0}_{1}", method.DeclaringType.Name, unmanagedMethodName);

            string qualMethodDecl = String.Format("{0} (THUNKCALL *{1})({2});",
                returnTypeString,
                qualMethodName,
                String.Join(", ", paramList));

            currentMethods.Add(new ThunkMethodInfo(
                method.Name,
                qualMethodName, qualMethodDecl,
                method.ToString(),
                method.IsGenericMethodDefinition));
        }

        static string FormatParameter(string typeName, string clrTypeName, string paramName)
        {
            if (wantComments)
                return String.Format("{0} /* {1} */ {2}",
                    typeName, clrTypeName, paramName);

            return String.Format("{0} {1}", typeName, paramName);
        }

        static string TypeToC(IKVM.Reflection.Type type)
        {
            if (type == TypeManager.VoidType)
                return "void";

            string monoType = TypeToMonoType(type);
            if (monoType != null)
                return monoType;

            if (type.IsArray)
                return ArrayTypeToC(type);

            if (type.IsByRef)
                return ByRefTypeToC(type);

            if (type.IsPrimitive)
                return PrimitiveTypeToC(type);

            if (type.IsValueType)
                return "MonoObject* /* BOX/UNBOX */";

            return "MonoObject*";
        }

        static string PrimitiveTypeToC(IKVM.Reflection.Type type)
        {
            var typeCode = IKVM.Reflection.Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return "MonoBoolean";
                case TypeCode.Byte:
                    return "uint8_t";
                case TypeCode.SByte:
                    return "int8_t";
                case TypeCode.Char:
                    return "uint16_t";
                case TypeCode.Double:
                    return "double";
                case TypeCode.Int16:
                    return "int16_t";
                case TypeCode.Int32:
                    return "int32_t";
                case TypeCode.Int64:
                    return "int64_t";
                case TypeCode.Single:
                    return "float";
                case TypeCode.UInt16:
                    return "uint16_t";
                case TypeCode.UInt32:
                    return "uint32_t";
                case TypeCode.UInt64:
                    return "uint64_t";
                case TypeCode.String:
                    return "MonoString*";
                case TypeCode.Object:
                    return "MonoObject*";
                default:
                    return "UnmappedPrimitiveType_" + typeCode;
            }
        }

        static string TypeToMonoType(IKVM.Reflection.Type type)
        {
            if (type == TypeManager.IntPtrType)
                return "void*";

            if (type == TypeManager.UIntPtrType)
                return "void*";

            if (type == TypeManager.StringType)
                return "MonoString*";

            if (type == TypeManager.ObjectType)
                return "MonoObject*";

            if (type == TypeManager.TypeType)
                return "MonoReflectionType*";

            if (type == TypeManager.MethodInfoType)
                return "MonoReflectionMethod*";

            if (type == TypeManager.FieldInfoType)
                return "MonoReflectionField*";

            if (type == TypeManager.PropertyInfoType)
                return "MonoReflectionProperty*";

            if (type == TypeManager.AssemblyType)
                return "MonoReflectionAssembly*";

            if (type == TypeManager.ModuleType)
                return "MonoReflectionModule*";

            return null;
        }

        static string ArrayTypeToC(IKVM.Reflection.Type type)
        {
            return "MonoArray*";
        }

        static string ByRefTypeToC(IKVM.Reflection.Type type)
        {
            return TypeToC(type.GetElementType()) + "*";
        }

        static string TypeToString(IKVM.Reflection.Type type)
        {
            if (type.Assembly == TypeManager.mscorlib)
                return type.Name;

            if (type.Assembly == currentAssembly)
                return type.Name;

            return type.FullName;
        }
    }
}
