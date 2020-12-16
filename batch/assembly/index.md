---
caption: Assembly
title: Running batch operation for assembly
description: Executing batch operation for all files or selected components in the SOLIDWORKS assembly
order: 3
image: property-page.png
---
Batch+ can be run directly within assembly environment.

![Menu command for Batch+ for assembly](menu-command.png)

In this case custom macro can be run on the selected components or all referenced documents.

![Batch+ Property Manager Page](property-page.png)

1. Components to run the macro on
1. Processes all dependent document of active assembly (including suppressed components)
1. List of macros to run
1. Optional macro arguments. Specify the arguments in the command line format if macro supports ones. Follow [Macro Arguments](/macro-arguments/) article of the instructions of how to use macro arguments.
1. Button to add macros to the scope
1. Specifies if documents needs to be activated (opened in their own windows). If this option is not selected macro will be run on invisible models. Follow [Model pointer in invisible mode](/batch/user-interface#model-pointer-in-invisible-mode) for more information.

Progress of the batch operation is reported in the progress bar in SOLIDWORKS icon. And the currently processed file is displayed in the status bar.

![Progress of batch operation](progress-status.png)

After completion the result, summary and log page is displayed.

![Results of batch operation](results.png)