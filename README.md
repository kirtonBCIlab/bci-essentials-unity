# BCIEssentials
A Unity package for development of BCI applications. This environment needs [BCI-essentials-Python](https://github.com/kirtonBCIlab/bci-essentials-python) to work properly.

## Getting Stdarted
- **[Wiki](https://github.com/kirtonBCIlab/bci-essentials-unity/wiki)** – More detailed installation instructions and tutorials.
- **[API documentation](https://kirtonbcilab.github.io/APIdocs-for-bci-essentials-unity)**

### Install into Unity Project
Follow these instructions to install BCI Essentials as a package to an existing Unity project.  For instructions on how to add packages hosted on Github using Unity's Package Manager [click here](https://docs.unity3d.com/Manual/upm-ui-giturl.html)

1. Install [LSL4Unity Package](https://github.com/labstreaminglayer/LSL4Unity) using git URL: `https://github.com/labstreaminglayer/LSL4Unity.git`
2. Install [BCI Essentials package](https://github.com/kirtonBCIlab/bci-essentials-unity) using git URL: `https://github.com/kirtonBCIlab/bci-essentials-unity.git?path=/Packages/com.bci4kids.bciessentials`

Note - tested with Uniy version 2021.3.15.f1, other editor versions might work but they have not been tested.

## Dependencies
- Math.NET Numerics: Included as `Assets/Plugins/MathNet.Numerics.dll` for numerical computations.

### Running Sample
1. Using Package Manager, select the BCI Essentials package and import the sample named `Original P300 Controller`
1. Open the test scene located in: `Assets > Samples > BCI Essentials > 1.0.0 > Original P300 Controller > Scenes > P300Training.Unity`
1. Select the `P300` object in the Hierarchy menu on the left hand side. This will open the inspector on the right hand side. Here you can modify the settings for the P300 controller.
1. Select the `play` button at the top-middle of the window.
1. You are now running a P300 BCI paradigm. You can use the keys detailed below to control the interface.

<div align="center">
  
| Key	| Action					|
| -----	| ---------------------------------------------	|
| T	| Begin training sequence		|
| S	| Start/stop stimulus				|
| I	| Begin interactive training (MI paradigm only)	|
| U	| Do user training, stimulus without BCI	|
| 0-9	| Select objects from 0-9 when stimulus is on	|

</div>

