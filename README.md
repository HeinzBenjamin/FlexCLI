# FlexCLI
FlexCLI is a C++/CLI interface to access the physics engine NVidia Flex from .Net environment. Furthermore the repository contains an implementation of FlexCLI in the form of a plugin for Grasshopper in Rhino3D. This plugin - called FlexHopper - provides the possiblity to use NVidia Flex physics simulation - via the FlexCLI pipeline - in the CAD software Rhino.<p>
FlexCLI is built against NVidia Flex release 1.1.0. NVidia Flex is patented property of NVidia. The author of this repository did not create or change NVidia proprietary code, nor is he the author of NVidia Flex. The author of this repository is the author of FlexCLI and FlexHopper only, both of which can found in the respective folders.<br>
For more information on NVidia Flex go here: https://developer.nvidia.com/flex and https://developer.nvidia.com/nvidia-flex-110-released<p><p>

FlexCLI runs on x64 architectures only. It was built against .Net 4.5.2<p>
Flex.sln contains FlexCLI and FlexHopper. Upon building the solution all compiled files will be stored inside "bin". Make sure to set your compiler platform to x64.<p>
FlexHopper was tested with Rhino5 64bit and Grasshopper 0.9.0076 WIP

# INSTRUCTIONS
Please follow the instructions under one of these options:<p>
<i><b>Option 1: Only use FlexHopper</b></i>
1. Go to the /bin folder in this repository and download all the files in there<br>
2. Put all of these files into your local Grasshopper/Library folder (usually in 'C:\Users\<your-user-name>\AppData\Roaming\Grasshopper\Libraries\'). You can make a subfolder in there if you want, but make sure that all files sit in the same directory.<br>
3. Start up Rhino and start using FlexHopper<br>
4. Check out the exmaple files in Example files/Flexhopper

<i><b>Option 2: Use FlexCLI to write your own implementation of NVidia Flex in .Net</i></b>
1. Go to the /bin folder in this repository and download all files apart from "FlexHopper.gha"
2. Put all of these files into one directory of your choice
3. Start using FlexCLI.dll

<i><b>Option 3: Download, compile and edit this repo</i></b>
1. git clone https://github.com/HeinzBenjamin/FlexCLI
2. Follow the instructions inside FlexCore110/include/README.md
3. Open Flex.sln -> Go to FlexHopper -> Properties -> Build Events -> Post-build event command line: Remove the last two lines
