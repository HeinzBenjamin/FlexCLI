<?xml version="1.0"?><doc>
<members>
<member name="M:FlexCLI.FlexScene.#ctor" decl="true" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.h" line="186">
<summary>Empty constructor</summary>
</member>
<member name="M:FlexCLI.FlexScene.NumParticles" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.h" line="189">
<summary>Number of all particles in the scene</summary>
</member>
<member name="M:FlexCLI.SimBuffers.Allocate" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.cpp" line="66">
Tells the host upon startup, how much memory it will need and reserves this memory
</member>
<member name="M:FlexCLI.SimBuffers.Destroy" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.cpp" line="100">
<summary>
Performs the following steps for every buffer: Check if pointer is 0; if it is, do nothing. If it is not, free buffer (NvFlex function) and set pointer to 0.
</summary>
</member>
<member name="M:FlexCLI.Flex.#ctor" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.cpp" line="229">
<summary>Create a default Flex engine object. This will initialize a solver, create buffers and set up default NvFlexParams.</summary>
</member>
<member name="M:FlexCLI.Flex.IsReady" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.cpp" line="311">
<summary>Returns true if pointers to library and solver objects are valid</summary>
</member>
<member name="M:FlexCLI.Flex.SetCollisionGeometry(FlexCLI.FlexCollisionGeometry)" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.cpp" line="318">
<summary>Register different collision geometries wrapped into the FlexCollisionGeometry class.</summary>
</member>
<member name="M:FlexCLI.Flex.SetParams(FlexCLI.FlexParams)" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.cpp" line="481">
<summary>Register simulation parameters using the FlexCLI.FlexParams class</summary>
</member>
<member name="M:FlexCLI.Flex.SetScene(FlexCLI.FlexScene)" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.cpp" line="541">
<summary>Register a simulation scenery using the FlexCLI.FlexScene class</summary>
</member>
</members>
</doc>