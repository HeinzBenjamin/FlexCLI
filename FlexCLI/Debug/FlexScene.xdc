<?xml version="1.0"?><doc>
<members>
<member name="M:FlexCLI.FlexScene.#ctor" decl="true" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.h" line="186">
<summary>Empty constructor</summary>
</member>
<member name="M:FlexCLI.FlexScene.NumParticles" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexcli.h" line="189">
<summary>Number of all particles in the scene</summary>
</member>
<member name="M:FlexCLI.FlexScene.#ctor" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexscene.cpp" line="6">
<summary>Empty constructor</summary>
</member>
<member name="M:FlexCLI.FlexScene.RegisterFluid(System.Single[],System.Single[],System.Single[],System.Int32)" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexscene.cpp" line="120">
<summary>
Specify one group of fluid particles
</summary>
<param name="positions">Flat array of particle positions [x, y, z]. Must be of length 3 * nr. of vertices</param>
<param name="velocities">Flat array of particle velocities [x, y, z]. Should be of same length as 'vertices'.</param>
<param name="inverseMass">Supply inverse mass per particle. Alternatively supply array of length one, value will be assigned to every vertex particle.</param>
<param name="groupIndex">A uniquely used index between 0 and 2^24. All particles in this group will be identified by the group index in the future.</param>
</member>
<member name="M:FlexCLI.FlexScene.RegisterRigidBody(System.Single[],System.Single[],System.Single[],System.Single[],System.Single,System.Int32)" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexscene.cpp" line="149">
<summary>
Specify one rigid body
</summary>
<param name="vertices">Flat array of vertex positions. Must be of length 3 * nr. of vertices</param>
<param name="vertexNormals">Flat array of normal vectors in vertices. Should be of same length as 'vertices'.</param>
<param name="velocity">Intial velocity acting on the rigid body.</param>
<param name="inverseMass">Supply inverse mass per particle. Alternatively supply array of length one, value will be assigned to every vertex particle.</param>
<param name="stiffness">Stiffness between 0.0 and 1.0</param>
<param name="groupIndex">A uniquely used index between 0 and 2^24. All particles in this object will be identified by the group index in the future.</param>
</member>
<member name="M:FlexCLI.FlexScene.RegisterSpringSystem(System.Single[],System.Single[],System.Single[],System.Int32[],System.Single[],System.Single[],System.Boolean,System.Int32[],System.Int32)" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexscene.cpp" line="246">
<summary>
Specify one spring system by spring pair indices
</summary>
<returns>The offset in spring indices resulting from previously registered spring systems. Use this to redraw the spring lines correctly later on.</returns>
</member>
<member name="M:FlexCLI.FlexScene.RegisterCloth(System.Single[],System.Single[],System.Single[],System.Int32[],System.Single[],System.Single,System.Single,System.Single,System.Int32[],System.Int32)" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexscene.cpp" line="301">
<summary>
Specify one cloth
</summary>
</member>
<member name="M:FlexCLI.FlexScene.RegisterCustomConstraints(System.Int32[],System.Int32[],System.Single,System.Int32[],System.Single[],System.Single[],System.Int32[],System.Single[])" decl="false" source="c:\users\bfelb\work\02-flexhopper\code\flexcli\flexcli\flexscene.cpp" line="474">
<returns>Return true, if registration was succesful. Returns false, if constraint indices exceeded particle count.</returns>
</member>
</members>
</doc>