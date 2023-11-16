# Export BOM

This VBA macro demonstrates how to export Bill Of Materials from the active assembly or part document via [BOM+](https://cadplus.xarial.com/bom/) API

BOM file will be saved to the same folder as the active model.

File will be named as the name of the active document followed by the value of the nominated custom property (e.g. **Revision**). Macro can be configured by changing the parameters below:

~~~ vb
Const BOM_TEMPLATE_NAME As String = "Full" 'Name of the BOM+ template
Const OUT_FILE_EXT As String = ".xlsx" 'Extension of the output file
Const SUFFIX_PRP_NAME = "Revision" 'Name of the custom property to add as the suffix
Const INCLUDE_ROOT As Boolean = True 'True to include the root item into the output
~~~