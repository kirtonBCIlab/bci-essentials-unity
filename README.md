# BCIEssentials
A Unity base environment for streamlined development of BCI applications. This environment needs [BCI-essentials-Python](https://github.com/kirtonBCIlab/bci-essentials-python) to work properly.

## Quick start guide

### Existing Unity Project
Follow these instructions to install BCI Essentials as a package to an existing Unity project.
For instructions on how to add packages hosted on Github using Unity's Package Manager [click here](https://docs.unity3d.com/Manual/upm-ui-giturl.html)

1. Install the [LSL4Unity Package](https://github.com/labstreaminglayer/LSL4Unity.git) (https://github.com/labstreaminglayer/LSL4Unity.git)
   - This a dependency. If it's not installed before the BCI Essentials package then installation will fail 
2. Install the [BCI Essentials package](https://github.com/kirtonBCIlab/bci-essentials-unity.git) (https://github.com/kirtonBCIlab/bci-essentials-unity.git)

### New Unity Project
Follow these steps to start a Unity project based on BCI-Essentials-Unity. Conversevely, you can add the desired prefabs 
and scripts to your existing project.

1. Download Unity hub from [here](https://unity.com/download) and install.
2. Sign in or create an account following the on-screen instructtions.
3. Skip the Unity editor installation.
4. Clone or download [BCI-essentials-Unity](https://github.com/kirtonBCIlab/bci-essentials-unity) and [BCI-essentials-Python](https://github.com/kirtonBCIlab/bci-essentials-python).
5. Follow instructions in BCI-essentials-Python to install it.
6. Install the Unity editor `2021.3.15.f1`. Note that other editor versions might work but they have not been tested.
	1. Select dev tools if needed. Alternatively, you can use [VS Code](https://code.visualstudio.com/download) to write and edit C# code.
	2. If you plan to build your application in Windows, select Windows build support (IL2CPP). Note that other build options might work but have not been tested.
	3. Skip installing any newer editor version if prompted.
7. Install the BCI Essentials package and its dependency [LSL4Unity](https://github.com/labstreaminglayer/LSL4Unity)
   - See the 'Existing Unity Project' guide above for more details 	
9. Open the test scene located in:

```
Assets > Samples > BCI Essentials > 1.0.0 > Original P300 Controller > Scenes > P300Training.Unity
```

9. Select the `P300` object in the Hierarchy menu on the left hand side. This will open the inspector on the right hand side. Here you can modify the settings for the P300 controller.
10. Select the `play` button at the top-middle of the window.
11. You are now running a P300 BCI paradigm. You can use the keys detailed below to control the interface.


<div align="center">

| Key	| Action										|
| -----	| ---------------------------------------------	|
| T		| Begin training sequence						|
| S		| Start/stop stimulus							|
| I		| Begin interactive training (MI paradigm only)	|
| U		| Do user training, stimulus without BCI		|
| 0-9	| Select objects from 0-9 when stimulus is on	|

</div>

## Notes

### Apple Silicon
To run on Apple silicon (ex: M1), the Lab Streaming Layer plugin library needs to be replaced:

1. Download the latest [liblsl release](https://github.com/sccn/liblsl/releases) for MacOS (arm64)

2. Unzip the archive and copy the lib/*.dylib files into ./Assets/Plugins/lib
