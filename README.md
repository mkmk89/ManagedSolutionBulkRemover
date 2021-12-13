[![Build Status](https://mkotfs.visualstudio.com/My%20Dynamics%20Developments/_apis/build/status/UnmanagedLayerBulkRemover%20Nuget%20publishing%20pipeline?branchName=master)](https://mkotfs.visualstudio.com/My%20Dynamics%20Developments/_build/latest?definitionId=8&branchName=master)
# Description
This is XrmToolbox plugin can remove unmanaged (active) layers from solution components in bulk. Just add components to solution, run the plugin and remove Active layers automatically.

# Instlalation
You can install the plugin through XrmToolBox Plugin Store

# Usage
Make sure you add only those components to the solution, which you want to remove unmanaged (active) layer from. You can narrow it down with a use of filter box (which is especially useful when you add attributes, but you want to exclude entity component type). When you click remove active layers, plugin will iterate through all components added to the solution and which are selected in the filter box and for each it will execute Remove Active Customizations action.

![](https://raw.githubusercontent.com/mkmk89/UnmanagedLayerBulkRemover/master/usage.gif)

# Contribution
Feel free to fork, contribute and submit pull requests
