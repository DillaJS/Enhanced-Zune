# Distribution decision and references

This local candidate is not published or uploaded. It excludes original Microsoft installation media and full Microsoft DLLs. Users supply legitimate original media or their existing compatible installation. A binary patch is a packaging mechanism, not proof that derivative code/artwork may be redistributed.

No redistribution authorization for Zune was established. Microsoft's general terms state that software is governed by its accompanying license and restrict redistribution outside that license. Public release needs review of the applicable Zune license and rights in the inherited dark artwork and derivative enhanced assets. No blanket open-source license is assigned to Microsoft material or third-party artwork in this project.

The available original Microsoft media was inspected locally: x64 4.8.2345.0; MSI ProductCode `{9B75648B-6C30-4A0D-9DE6-0D09D20AF5A5}`. Its Authenticode signature validated on this host. The MSI has no license/dialog UI; the clean-install route therefore launches the verified original Microsoft setup from complete media to retain its own license and prerequisite flow. No MSI or original setup is included in the candidate.

Setup uses Windows-provided .NET Framework, Windows Forms and system APIs. The setup EXE embeds a self-contained Microsoft .NET audio runtime and NAudio for the optional crossfade component. It does not embed Python, Mono.Cecil, Inno Setup, NSIS, community-service modifications or diagnostic tools. Build-time tools are separate from runtime dependencies.

Primary references checked during preparation:

- [Microsoft Terms of Use â€” software and accompanying license](https://www.microsoft.com/en-us/legal/terms-of-use)
- [Microsoft .NET Framework 3.5 installation guidance](https://learn.microsoft.com/en-us/dotnet/framework/install/dotnet-35-windows)
- [Microsoft Windows 11 .NET Framework 3.5 guidance](https://learn.microsoft.com/en-us/dotnet/framework/install/dotnet-35-windows-11)
- [Microsoft Windows Sandbox configuration](https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-configure-using-wsb-file)

The local MSI metadata, signature result, exact original-media hashes and source policy results are preserved with private engineering evidence. Legal clearance remains open; the links above do not supply it.

RC6 optionally opens https://www.zuneupdate.com/resources/zune-software.zip in the user’s browser. This is a community archive, not a current Microsoft service. The separate original package is not redistributed inside this enhancement installer; its known hash and every extracted x64 installation file are checked before setup is offered.


RC7 audio dependencies: [NAudio 2.2.1](https://github.com/naudio/NAudio) (MIT) and [Microsoft .NET runtime](https://github.com/dotnet/runtime) / [Windows Desktop runtime](https://github.com/dotnet/windowsdesktop) 8.0.31. Required license and third-party notices are installed as ZuneCrossfade-Licenses.txt and included in the source package. These open-source runtime licenses do not license inherited Microsoft Zune code or artwork.
