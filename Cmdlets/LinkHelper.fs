module internal OpilioCraft.MetadataEngine.Cmdlets.LinkHelper

open System
open System.Runtime.InteropServices

// Win32 API
[<DllImport("Kernel32.dll", CharSet = CharSet.Unicode )>]
extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

// link creation
let createHardLink (existingPath : string) (newPath : string) =
    CreateHardLink(newPath, existingPath, IntPtr.Zero) |> ignore

let createSymbolicLink (existingPath : string) (newPath : string) =
    IO.File.CreateSymbolicLink(newPath, existingPath) |> ignore
