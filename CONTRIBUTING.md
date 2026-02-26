At the moment, BCI Essentials is largely a suite of internal research tools.
Contributions are welcome but shouldn't expect regular or timely attention.


# Contributor Setup
*shell scripts are provided below to automate this process*

This repository hosts only a Unity package.

In order to streamline development on the package in the Unity Editor, contributors are recommended to set up a local skeleton project within which to host and work on the package.

The following steps will guide you through the creation of a skeleton project to host this package while in development:
1. Create an empty Unity project
2. Install the [LSL4Unity Package](https://github.com/labstreaminglayer/LSL4Unity) using git URL: `https://github.com/labstreaminglayer/LSL4Unity.git` through the Unity package manager
3. Clone this repository into your project's `Packages` folder as `com.bci4kids.bciessentials` - ***The Folder Name Has to Match***
    - `git clone https://github.com/kirtonBCIlab/bci-essentials-unity.git Packages\com.bci4kids.bciessentials`
4. Make a symlink or directory junction `Assets/Samples` pointing to "Your/Unity/Project/Path/Packages/com.bci4kids.bciessentials/Samples~"
   - Windows: `mklink /d Assets\Samples "%cd%\Packages\com.bci4kids.bciessentials\Samples~"
   - MacOs/Linux: `ln -s -- "${pwd}/Packages/com.bci4kids.bciessentials/Samples~" Assets/Samples`

*This will enable you to see and edit project samples in the Unity editor*

## Automated Shell Scripts
The following scripts will automate the following setup process:
1. Create an empty Unity project
2. Clone both LSL4Unity and BCI-Essentials Unity as local custom packages
3. Make a symbolic link from Samples~ to Assets/Samples in your new project

***Please ensure that the project path is set as desired and Unity version is one you have installed.***

***The script will wait for Unity to create the project in a new window and for you to close the Unity editor before finishing the setup process.***

### Windows:
1. Copy text content into `bci-essentials_unity_setup.bat` *(or any other name.bat)*
    - Update project path and Unity version variables as needed
2. Move new batch script to your Unity projects directory
3. Run the batch script by double-clicking it
4. Close the Unity window once the new project is open

*If the script fails to create a symbolic link, you may have to enable developer mode or run it as an administrator*
```bat
@echo off
set projectPath=BCI Essentials Unity
set unityVersion=2021.3.15f1
set unityPath=C:\Program Files\Unity\Hub\Editor\%unityVersion%\Editor\Unity.exe

if exist "%projectPath%\" (
    echo Project already exists...
    goto :openProject
)

echo Creating skeleton project to house the package...
echo Please close the unity window once it is complete

"%unityPath%" -createProject "%projectPath%"
cd "%projectPath%" || goto :error

echo Project created.
echo Cloning LSL4Unity and BCI-Essentials Unity as local packages...

git clone https://github.com/labstreaminglayer/LSL4Unity.git Packages\com.labstreaminglayer.lsl4unity
git clone https://github.com/kirtonBCIlab/bci-essentials-unity.git Packages\com.bci4kids.bciessentials

echo Packages cloned.
echo Creating symbolic link to Samples~ folder...

mklink /d Assets\Samples "%cd%\Packages\com.bci4kids.bciessentials\Samples~" || goto :junctionError

echo Link Created.
echo You're good to go!


:openProject
choice /c yn /m "Would you like to open the project in Unity"
if ERRORLEVEL ==2 goto :end

start "" "%unityPath%" -projectPath "%projectPath%"
goto :end


:error
echo Failed to create project.
pause
goto :end

:junctionError
echo Failed to create directory junction for samples.
pause
goto :end

:end
```

### MacOS *(untested)*
1. Copy text content into `bci-essentials_unity_setup.sh` *(or any other name.sh)*
    - Update project path and Unity version variables as needed
2. Move new bash script to your Unity projects directory
3. Run the bash script by right clicking on it and selecting run
4. Close the Unity window once the new project is open

*This script has not been tested*
```bash
#!/bin/bash
PROJECT_PATH="BCI Essentials Unity"
UNITY_VERSION=2021.3.15f1
UNITY_PATH="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"

echo "Creating skeleton project to house the package..."
echo "Please close the unity window once it is complete"

"${UNITY_PATH}" -createProject "${PROJECT_PATH}"

if [ -d "${PROJECT_PATH}" ]; then
    echo "Project created."
    cd "${PROJECT_PATH}"

    echo "Cloning LSL4Unity and BCI-Essentials Unity as local packages..."

    git clone https://github.com/labstreaminglayer/LSL4Unity.git Packages/com.labstreaminglayer.lsl4unity
    git clone https://github.com/kirtonBCIlab/bci-essentials-unity.git Packages/com.bci4kids.bciessentials
    
    echo "Packages cloned."
    echo "Creating symbolic link to Samples~ folder..."

    ln -s Packages/com.bci4kids.bciessentials/Samples~ Assets/Samples
else
    echo "Failed to create project."
fi
```