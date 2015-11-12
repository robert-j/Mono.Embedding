# Mono.Embedding

## ThunkTool

This utility is generating C/C++ source code from an assembly which
contains Thunk attributes.

When applied to members, `Thunk` attributes (see `ThunkAttribute` class)
are instrucing  ThunkTool to generate C/C++ callable thunks from these members.

The ThunkAttribute class can (and should) be included inline
in the assembly because ThunkTool is looking for this attribute
by name.

### What does the tool generate?

- Thunks for all members marked with `[Thunk]`

- Initialization function for the assembly:

    `$(Assembly_Name)_Init((MonoAssembly *assembly);`

- Function which invokes the Main() entry point of an assembly:

    `$(Assembly_Name)_Exec(void);`


### Usage

`mono ThunkTool.exe --output prefix [--comments]  assembly.{dll|exe}`

The output will be stored into `prefix.c` and `prefix.h`.

### Example

#### Input code

    // C##
    // assembly foo.dll
    namespace Foo {
      public class ExportMe {
        [Thunk]
        public int Method() { ... }
   
        [Thunk]
        public string Property { get { ... } }   
  
        // emit a more specific type than intptr    
        [Thunk (ReturnType = "MonoType *")]
        public IntPtr GetTypeHandle(Type t)
        {
            return t.TypeHandle.Value;
        }
        
        // assembly entry point
        static void Main ()
        {
        }
      }
    }


#### Invoke ThunkTool

`mono ThunkTool.exe --output=foo foo.dll`

  
#### Generated C/C++ header

    // `foo.h`
    ...
    MonoClass *ExportMe__Class;   // Foo.ExportMe class
    int32_t (THUNKCALL *ExportMe_Method)(MonoObject **ex);
    MonoString* (THUNKCALL *ExportMe_get_Property)(MonoObject **ex);
    ...
  
    MonoAssembly *Foo_Assembly;
    MonoImage *Foo_Image;

    // initalization function.
    void Foo_Init (MonoAssembly *assembly);

    // invokes the entry point of the assembly
    void Foo_Exec (void);


In your own C++ code you should include the header, initialize the
Mono runtime, load the assembly and pass it to `$(Assembly_Name)_Init ()`
function. After that, all members attributed with `[Thunk]` are
available for invocation. Additionally, all involved classes
are available as `$(Assembly_Name)_$(ClassName)__Class` variables.

