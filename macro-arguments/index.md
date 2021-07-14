---
title: Managing VBA macro arguments in CAD+
caption: Macro Arguments
description: Instructions of how to pass and consume custom VBA macro arguments using CAD+
order: 6
---
## Passing parameters to the macro

Coming soon...

## Retrieving parameters in the macro

When parameters are passed to the macro via [Toolbar+](/toolbar/), [Batch+](/batch/) or from custom invocation, macro can retrieve them by accessing the **CadPlus.MacroRunner.Sw::PopParameter** method.

Call **::Get** method to get specific parameter by name.

~~~ vb
Dim macroRunner As Object
Set macroRunner = CreateObject("CadPlus.MacroRunner.Sw")

Dim param As Object
Set param = macroRunner.PopParameter(swApp)
    
Dim vArgs As Variant
vArgs = param.Get("Args")

Dim text As String
text = param.Get("Text")

Dim swModel As SldWorks.ModelDoc2
Set swModel = param.Get("Model")
~~~

### Setting result

Coming soon...