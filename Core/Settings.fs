namespace OpilioCraft.MetadataEngine

open System

// ------------------------------------------------------------------------------------------------

[<RequireQualifiedAccess>]
module Settings =
    let FrameworkVersion = Version(1, 0)

    // location of runtime; e.g. for side-by-side apps
    let AssemblyLocation = Uri(Reflection.Assembly.GetExecutingAssembly().Location).LocalPath
    let RuntimeBase = IO.Path.GetDirectoryName(AssemblyLocation)

// ------------------------------------------------------------------------------------------------

[<RequireQualifiedAccess>]
module SlotPrefix =
     /// Prefix of all entries returned by ExifTool
    [<Literal>]
    let ExifTool = "ExifTool:"

[<RequireQualifiedAccess>]
module Slot =
    /// Camera maker and model combined into one field
    [<Literal>]
    let Camera = "Camera"
    
    /// DateTime when the item was created
    [<Literal>]
    let DateTaken = "DateTaken"

     /// Date taken without any corrections as stored in metadata
    [<Literal>]
    let DateTakenOriginal = "DateTaken:Original"
    /// Offset as TimeSpan to be added to DateTakenOriginal
    [<Literal>]
    let DateTakenOffset = "DateTaken:Offset"

     /// Used to show whether we have data from ExifTool at hand
    [<Literal>]
    let ExifTool = "ExifTool"

    /// Owner of the content
    [<Literal>]
    let Owner = "Owner"

    /// Sequence number of a picture in a series
    [<Literal>]
    let SequenceNo = "ExifTool:MakerNotes:SequenceImageNumber"

    /// Milliseconds of DateTimeOriginal
    [<Literal>]
    let SubSeconds ="ExifTool:EXIF:SubSecTimeOriginal"

[<RequireQualifiedAccess>]
module RuleName =
    [<Literal>]
    let GuessCamera = "GuessCamera"

    [<Literal>]
    let GuessOwner = "GuessOwner"

    [<Literal>]
    let DetermineDateTaken = "DetermineDateTaken"
