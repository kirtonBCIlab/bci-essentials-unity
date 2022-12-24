# BCIEssentials
A Unity base environment for streamlined development of BCI applications.

Pairs nicely with [bci-essentials-python](https://github.com/kirtonBCIlab/bci-essentials-python)

## Installing the BCI Essentials Package
You can install the BCI Essentials package through Unity's Package Manager
1. Add [LSL4Unity](https://github.com/labstreaminglayer/LSL4Unity.git) package
2. Install the BCI Essentials using the Package Manager
   1. Add using Git URL `https://github.com/kirtonBCIlab/bci-essentials-unity.git?path=/packages/com.bci4kids.bciessentials`

___

### Note - running on Apple silicon:
To run on Apple silicon (ex: M1), the Lab Streaming Layer plugin library needs to be replaced:

1. Download the latest [liblsl release](https://github.com/sccn/liblsl/releases) for MacOS (arm64)

2. Unzip the archive and copy the lib/*.dylib files into ./Assets/Plugins/lib

