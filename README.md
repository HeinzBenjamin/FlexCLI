<a href="https://zenodo.org/badge/latestdoi/95669350"><img src="https://zenodo.org/badge/95669350.svg" alt="DOI"></a>


# FlexCLI
FlexCLI is a C++/CLI interface to access the physics engine NVidia Flex from the .Net environment. Furthermore the repository contains an implementation of FlexCLI in the form of a plugin for Grasshopper in Rhino3D. This plugin - called FlexHopper - provides the possiblity to use NVidia Flex physics simulation - via the FlexCLI pipeline - in the CAD software Rhino.<p>
FlexCLI is built against NVidia Flex release 1.1.0. NVidia Flex is patented property of NVidia. The author of this repository did not create or change NVidia proprietary code, nor is he the author of NVidia Flex. The author of this repository is the author of FlexCLI and FlexHopper only, both of which can found in the respective folders. The GPL-3.0 license mentioned in this repo applies <b>only</b> to FlexCLI and FlexHopper and <b>not</b> to Nvidia proprietary code (anything inside the folder FlexCore110). There's a respective license to be found in that folder.<br>
For more information on NVidia Flex go here: https://developer.nvidia.com/flex and https://developer.nvidia.com/nvidia-flex-110-released<p><p>

FlexCLI runs on x64 architectures only. It was built against .Net 4.5.2<p>
Flex.sln contains FlexCLI and FlexHopper. Upon building the solution all compiled files will be stored inside "bin". Make sure to set your compiler platform to x64.<p>
FlexHopper was tested with Rhino 6 64bit and Grasshopper 1.0.0076

Contact info:<br>
benjamin@felbrich.com<br>
flexhopper@felbrich.com<br>
https://www.linkedin.com/in/benjamin-felbrich/ <br>
https://twitter.com/BFelbrich <br>
    
# HARDWARE REQUIREMENTS NVIDIA FLEX
1. A <b>dedicated</b> NVidia and AMD graphics card supporting DirectX11 and running one of the following drivers:<br>
Nvidia Geforce Game Ready Driver 372.90 or above<br>
AMD Radeon Driver version 16.9.1 or above<br>
Onboard graphic chips like Intel HD Graphics 4000 are <b>not</b> supported and might crash your system
	
# INSTRUCTIONS
Please follow the instructions under one of these options:<p>
<i><b>Option 1: Only use FlexHopper</b></i>
1. Make sure your machine fulfills the hardware requirements (see above).
2. Make sure you have the latest version of Rhino 6 <b>64bit</b> along with the latest version of Grasshopper installed (in Rhino click "Help" > "Check for Updates")
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
  5. Start up Rhino 6 in <b>64bit</b> Mode and start using FlexHopper<br>
6. Check out the example files in Example files/Flexhopper. But don't rely too much on them, they may be outdated (sorry)

<i><b>Option 2: Use FlexCLI to write your own implementation of NVidia Flex in .Net</i></b>
1. Go to the /bin folder in this repository and download all files apart from "FlexHopper.gha"
2. Put all of these files into one directory of your choice
3. Start using FlexCLI.dll in .NET

<i><b>Option 3: Download, compile and edit this repo</i></b>
1. git clone https://github.com/HeinzBenjamin/FlexCLI
2. Follow the instructions inside FlexCore110/include/README.md

# COMMON ERRORS
FlexHopper only works with Rhino 6 64bit.<br>
If you receive an error message saying that FlexCLI or one of its dependecies could not be loaded, make sure to:<br>
A. Unlock all FlexHopper related files in the Library folder (Right click -> properties)<br>
B. Update your Rhino 6 to the latest version.<br>
C. Start up Rhino 6 in 64bit mode<br>
If your machine crashes upon resetting the FlexHopper engine:<br>
A. Make sure your computer fulfills the hardware requirements (see above)<br>
B. Consider reducing the memory your FlexHopper requires by adjusting the memQ input in the Flex Solver Options accordingly

# CITATION


<a href="https://zenodo.org/badge/latestdoi/95669350"><img src="https://zenodo.org/badge/95669350.svg" alt="DOI"></a>
>## Cite as
> HeinzBenjamin. (2019, July 30). HeinzBenjamin/FlexCLI: FlexCLI - FlexHopper (Version v1.1.2). Zenodo. http://doi.org/10.5281/zenodo.3355744
>## BibTex
> @misc{heinzbenjamin_2019_3355744,<br>
>   author       = {HeinzBenjamin},<br>
>   title        = {HeinzBenjamin/FlexCLI: FlexCLI - FlexHopper},<br>
>   month        = jul,<br>
>   year         = 2019,<br>
>   doi          = {10.5281/zenodo.3355744},<br>
>   url          = {https://doi.org/10.5281/zenodo.3355744}<br>
> }
