# BCIEssentials
A Unity base environment for streamlined development of BCI applications. This environment needs [BCI-essentials-Python](https://github.com/kirtonBCIlab/bci-essentials-python) to work properly.

## Quick start guide
Follow these steps to start a Unity project based on BCI-Essentials-Unity. Conversevely, you can add the desired prefabs 
and scripts to your existing project.

1. Download Unity hub from [here](https://unity.com/download) and install.
2. Sign in or create an account following the on-screen instructtions.
3. Skip the Unity editor installation.
4. Clone or download [BCI-essentials-Unity](https://github.com/kirtonBCIlab/bci-essentials-unity) and [BCI-essentials-Python](https://github.com/kirtonBCIlab/bci-essentials-python).
5. Follow instructions in BCI-essentials-Python to install it.
6. In Unity hub select `Projects -> Open` and select the folder where you have cloned BCI-essentials-Unity.
7. Install the Unity editor `2021.3.15.f1`. Note that other editor versions might work but they have not been tested.
	1. Select dev tools if needed. Alternatively, you can use [VS Code](https://code.visualstudio.com/download) to write and edit C# code.
	2. If you plan to build your application in Windows, select Windows build support (IL2CPP). Note that other build options might work but have not been tested.
	3. Skip installing any newer editor version if prompted.
8. Open the test scene located in:

```
Assets > Samples > BCI Essentials > 1.0.0 > Original P300 Controller > Scenes > P300Training.Unity
```

9. Select the `P300` object in the Hierarchy menu on the left hand side. This will open the inspector on the right hand side. Here you can modify the settings for the P300 controller.
10. Select the `play` button at the top-middle of the window.
11. You are now running a P300 BCI paradigm. You can use the keys detailed below to control the interface.


| Key	| Action										|
| -----	| ---------------------------------------------	|
| T		| Begin training sequence						|
| S		| Start/stop stimulus							|
| I		| Begin interactive training (MI paradigm only)	|
| U		| Do user training, stimulus without BCI		|
| 0-9	| Select objects from 0-9 when stimulus is on	|

## Notes

### Apple Silicon
To run on Apple silicon (ex: M1), the Lab Streaming Layer plugin library needs to be replaced:

1. Download the latest [liblsl release](https://github.com/sccn/liblsl/releases) for MacOS (arm64)

2. Unzip the archive and copy the lib/*.dylib files into ./Assets/Plugins/lib