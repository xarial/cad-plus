# CAD+ Toolset API (CAD++)

CAD+ Toolset enables extensibility via custom modules, extension of the existing modules and COM API

CAD++ SDK is available in the [Xarial.CadPlusPlus](https://www.nuget.org/packages/Xarial.CadPlusPlus) nuget package

COM APIs are exposed in the **\[CAD+ Toolset Installation Folder\]\CadPlusSwAddInComAPI.tlb** type library

CAD++ is available in Standard edition of CAD+ Toolset or higher.

# CAD+ Macro Runner

CAD+ enhances the macro execution process when used in **Toolbar+**, **Batch+** (Integrated, Stand-Alone and PDM Professional)

## VBA Macros

COM APIs for VBA macros are exposed in the **\[CAD+ Toolset Installation Folder\]\Xarial.CadPlus.MacroRunner.tlb** type library

* Macro can accept arguments with expressions
    * Custom variable can be handled in the expression
* Macro can return the operation result
* Macro can output log and trace messages
* Macro can report custom status messages

## xCAD.NET Macros

* Macros can be created in C# or VB.NET via CAD++ SDK which is available in the [Xarial.CadPlusPlus](https://www.nuget.org/packages/Xarial.CadPlusPlus) nuget package