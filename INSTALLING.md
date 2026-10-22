# Installation and updates

## Requirements

- Windows 10 or 11 on x64 hardware. This enhancement package does not support 32-bit Windows, Windows 7 or native ARM64 installations.
- Original **x64 Zune 4.8.2345** files. Microsoft media is not bundled with the enhancement installer.
- Required Windows media components and .NET Framework 3.5. Setup checks for these and offers applicable assistance/instructions. Installing prerequisites may need Internet access and a Windows restart. New Windows builds may require Microsoft's separate .NET 3.5 installer; follow setup's guidance.

## First installation

1. Download the desired Enhanced Zune installer from this repository's Releases page. RC7 repairs the Repeat One issue present in RC6.2; read that release's notes and limitations.
2. Verify its SHA-256 against the release's `SHA256SUMS.txt` if desired. PowerShell: `Get-FileHash .\Enhanced-Zune-Setup-1.0.0-rc.7.exe -Algorithm SHA256`.
3. Close Zune and run setup. Approve Windows elevation when requested.
4. If original Zune is missing, use **Download Zune**, then **Install downloaded Zune** and select the supported complete ZIP or large `ZunePackage.exe`. Manual extraction is not required. The download is a separate community-hosted archive; its availability is outside this project.
5. Complete Microsoft's original setup, then allow the enhancement installer to continue. If a Windows prerequisite/restart is required, follow the displayed instructions and reopen setup afterward.

## Update an earlier Enhanced Zune version

1. Close Zune and keep the existing verified backup.
2. Use **Restore originals** in the new setup. Custom backgrounds are exported during restoration; they may need to be selected again afterward.
3. Use **Install / verify** to apply the newer version.

Do not manually remove helpers or bypass an unrecognized-file error. A mismatched/modified installation may need its own matching backup. Music/library files are outside the installer transaction by design; this does not replace personal backups.

## Recovery and restarts

Use setup's repair/restore options and its matching verified backup. If interrupted, reopen setup and follow its recovery guidance. Never delete a recovery backup just to dismiss an error.

The installer never automatically restarts Windows. Windows media-component installation can require a restart even when Zune appears to open. App display changes have their own restart notices. Automatic Zune launch after installation is suppressed when setup itself is elevated; use Start after closing setup in that case.

## Reporting a failure

Use Issues and include the enhancement version, Windows version/build and system type, the exact error code/message, and the step that failed. Say whether you selected the ZIP through this installer or launched Microsoft setup separately. Remove personal paths/account details before attaching logs. Do not send music, Microsoft DLLs or registry exports. Avoid deleting registry entries as a troubleshooting guess.
