NVIDIA Flex - 1.1.0
===================

Flex is a particle-based simulation library designed for real-time applications.
Please see the programmer's manual included in this release package for more information on
the solver API and usage.

The latest pre-built binary release can be found in on the NVIDIA Developer Zone:

https://developer.nvidia.com/flex

Supported Platforms
-------------------

* Windows 32/64 bit (CUDA, DX11, DX12)
* Linux 64 bit (CUDA, tested with Ubuntu 14.04 LTS and Mint 17.2 Rafaela)

Requirements
------------

A D3D11 capable graphics card with the following driver versions:

* NVIDIA GeForce Game Ready Driver 372.90 or above
* AMD Radeon Software Version 16.9.1 or above
* Intel® Graphics Version 15.33.43.4425 or above

To build the demo at least one of the following is required:

* Microsoft Visual Studio 2013
* Microsoft Visual Studio 2015
* g++ 4.6.3 or higher

And either: 

* CUDA 8.0.44 toolkit
* DirectX 11/12 SDK


Known Issues
============

* Crash with inflatable scenes on Intel HD Graphics 530
