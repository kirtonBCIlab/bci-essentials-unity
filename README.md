# BCIEssentials
A Unity base environment for streamlined development of BCI applications.

Pairs nicely with [bci-essentials-python](https://github.com/kirtonBCIlab/bci-essentials-python)

## Note - running on Apple silicon:
To run on Apple silicon (ex: M1), the Lab Streaming Layer plugin library needs to be replaced:

1. Download the latest [liblsl release](https://github.com/sccn/liblsl/releases) for MacOS (arm-64)

2. Unzip the archive and copy the lib/*.dylib files into ./Assets/Plugins/lib

