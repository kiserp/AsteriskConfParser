namespace AsteriskConfigParser

open System.IO
open System.Collections

module public FileReader =

    let GetFileListingByMask _directory mask =
        try
            Directory.GetFiles(_directory, mask)
        with
            | ex -> printf "An exeption occured %s" ex.Message ; [||]






    // let rec ReadAllFilesFromListing fileListing lines =
    //     match fileListing with
    //     | [] -> lines
    //     | head::tail ->
    //         ReadAllFilesFromListing tail (JoinPreviousLinesAndFile lines head)
