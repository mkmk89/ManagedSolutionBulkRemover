[![Build Status](https://dev.azure.com/maciejkornacki/My%20Dynamics%20Developments/_apis/build/status/UnmanagedLayerBulkRemover%20Nuget%20publishing%20pipeline?branchName=master)](https://dev.azure.com/maciejkornacki/My%20Dynamics%20Developments/_build/latest?definitionId=8&branchName=master)
# Description
This is XrmToolbox plugin can delete managed solutionsin bulk. Select solutions you want to delete and it will iterate through them, considenring dependencies order.

# Instlalation
You can install the plugin through XrmToolBox Plugin Store

# Usage
Make sure you select only solutions you want to delete (with ctrl key pressed). Plugin will never delete solution which hasn't been selected. When you click Delete Solutions plugin will query each solution dependencies and will start deleting those without dependencies first. Than it will do another iteration of dependencies check and will process those which don't have any dependencies this time. Plugin will log all deletion errors in case you need to resolve dependencies manually. Tool is designed to run long running job of solutions deletion in bulk, however if any dependencies exist and can't be deleted, plugin will log errors, so issues resolution needs to be manual.   

![](https://raw.githubusercontent.com/mkmk89/ManagedSolutionBulkRemover/master/usage.gif)

# Contribution
Feel free to fork, contribute and submit pull requests
