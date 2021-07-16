# UnityForgeImporter

[![GitHub issues](https://img.shields.io/github/issues/Piro-CIE/UnityForgeImporter)](https://github.com/Piro-CIE/UnityForgeImporter/issues)
![platforms](https://img.shields.io/badge/platform-windows%20%7C%20osx%20%7C%20linux-lightgray.svg)
[![license](https://img.shields.io/badge/license-MIT-blue.svg)](http://opensource.org/licenses/MIT)

![Forge & Unity logos](./Documentation/forge_unity_logos.png)

A simple importer for [Unity][unity] to load [Autodesk Forge][autodeskforge] SVF models in runtime.

It has been developed using the [Forge-Convert-Utils][forgeconvertutils] package. Most of this code is only Typescript to C# adaptation and the majority of the **SVF** reading logic is preserved.

## Usage

This library only works with local **SVF** files.

1. Download or clone this repo

2. Download your **SVF** using [Forge-Convert-Utils][forgeconvertutils] or [Extract-Autodesk](https://github.com/cyrillef/extract.autodesk.io). Samples can downloaded from [Forge-RCDB Gallery](https://forge-rcdb.autodesk.io/gallery)

3. Put **SVF** files into your Unity project (StreamingAssets folder for exemple)

4. Create an instance of the `ForgeSvfImporter` and load the model with the `ReadSvf` method

```
    ForgeSvfImporter svfImporter = new ForgeSvfImporter();
    svfImporter.ReadSvf(svf-model-path);
```

## Unity project

This project has been developed under Unity 2019.4.23f1. It should work on newer versions of Unity but not tested yet.

## Dependencies

- [NewtonSoft Json](https://www.newtonsoft.com/json)
- [SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
## License

This project is licensed under the terms of the MIT License. Please see the [LICENSE](LICENSE) file for full details.
## Trademarks

*Unity* is a registered trademark of [Unity Technologies][unity] <br>
*Forge* is a registered trademark of [Autodesk][autodeskforge]

## Written by

Alexandre Piro <br>
Piro CIE


[unity]: https://unity.com
[autodeskforge]: https://forge.autodesk.com/
[forgeconvertutils]: https://github.com/petrbroz/forge-convert-utils
