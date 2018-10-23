# FlexCLI
FlexCLI is a C++/CLI interface to access the physics engine NVidia Flex from .Net environment. Furthermore the repository contains an implementation of FlexCLI in the form of a plugin for Grasshopper in Rhino3D. This plugin - called FlexHopper - provides the possiblity to use NVidia Flex physics simulation - via the FlexCLI pipeline - in the CAD software Rhino.<p>
FlexCLI is built against NVidia Flex release 1.1.0. NVidia Flex is patented property of NVidia. The author of this repository did not create or change NVidia proprietary code, nor is he the author of NVidia Flex. The author of this repository is the author of FlexCLI and FlexHopper only, both of which can found in the respective folders.<br>
For more information on NVidia Flex go here: https://developer.nvidia.com/flex and https://developer.nvidia.com/nvidia-flex-110-released<p><p>

FlexCLI runs on x64 architectures only. It was built against .Net 4.5.2<p>
Flex.sln contains FlexCLI and FlexHopper. Upon building the solution all compiled files will be stored inside "bin". Make sure to set your compiler platform to x64.<p>
FlexHopper was tested with Rhino5 64bit and Grasshopper 0.9.0076 WIP

Twitter: @HeinzPfeiffer<p>

# INSTRUCTIONS
Please follow the instructions under one of these options:<p>
<i><b>Option 1: Only use FlexHopper</b></i>
1. Make sure your machine fulfills the hardware requirements (see below).
2. Make sure you have Rhino 5 64bit or Rhino 6 64bit along with the latest version of Grasshopper installed.
3. Download the package:<br>
- Go to www.food4rhino.com/app/flexhopper<br>
- Download latest version <br>
- Unzip the package, it should contain:<br>
  - FlexHopper.gha<br>
  - FlexCLI.dll<br>
  - NvFlexExtReleaseD3D_x64.dll<br>
  - NvFlexReleaseD3D_x64.dll<br>
  - amd_ags_x64.dll<br>
- unpack all files into your Grasshopper Components Folder (usually in 'C:\Users\YOUR-USER-NAME\AppData\Roaming\Grasshopper\Libraries\')<br>
  ... if you can't find that folder, open Grasshopper, click "File > Special Folders > Components Folder"<br>
  (Alternatively to food4rhino you can download the necessary files from the "/bin/Release" folder on this very website)<br>
4. Unlock all .dll and .gha files (Right click each of them individually -> Properties -> Tick Unlock)
  5. Start up Rhino 5 or 6 in <b>64 bit</b> Mode and start using FlexHopper<br>
6. Check out the exmaple files in Example files/Flexhopper

<i><b>Option 2: Use FlexCLI to write your own implementation of NVidia Flex in .Net</i></b>
1. Go to the /bin folder in this repository and download all files apart from "FlexHopper.gha"
2. Put all of these files into one directory of your choice
3. Start using FlexCLI.dll

<i><b>Option 3: Download, compile and edit this repo</i></b>
1. git clone https://github.com/HeinzBenjamin/FlexCLI
2. Follow the instructions inside FlexCore110/include/README.md
3. Open Flex.sln -> Go to FlexHopper -> Properties -> Build Events -> Post-build event command line: Remove the last two lines

# NOTES
FlexHopper only works with Rhino 64bit.<br>
If you receive an error message saying that FlexCLI or one of its dependecies could not be loaded, make sure to:<br>
A. Unlock all FlexHopper related files in the Library folder (Right click -> properties)
B. Start up "Rhinoceros 5 (64bit)" and not the default "Rhinoceros 5"

# HARDWARE REQUIREMENTS NVIDIA FLEX
1. A dedicated NVidia and AMD graphics card supporting DirectX11 and running one of the following drivers:<br>
Nvidia Geforce Game Ready Driver 372.90 or above<br>
AMD Radeon Driver version 16.9.1 or above
