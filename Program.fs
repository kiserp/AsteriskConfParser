// Learn more about F# at http://fsharp.org

open AsteriskConfigParser.FileReader
open AsteriskConfigParser
open AsteriskConfigParser.AsteriskDialplanObjects
open System
open System.IO

[<EntryPoint>]
let main argv =
    printfn "Enter Path: "
    let path = "/home/LDAP/preslerks/devel/svn/asterisk/"// Console.ReadLine()
    printfn "Enter mask: "
    let mask = "extensions*.conf" //Console.ReadLine()

    let parsed =
        GetFileListingByMask path mask
        |> Array.Parallel.map(fun path ->
                File.ReadAllLines path
                |> Array.toList)
        |> Array.Parallel.map(fun lineList ->
               PreprocessExtensionLines lineList "" [])
        |> List.concat
        |> GetExtenStionsFromOneLines
        |> GetContextsFromExtensions


    let GetLocalEdges extensions (ctxName: string) =
        extensions
        |> List.collect (fun ext ->
            ext.ContextExtenLinks
            |> List.map(
                fun lnk ->
                    let ctx, exten = lnk
                    let aNode = if  ext.Name.Length > 1 then  (sprintf "%s  ~  %s" ctxName ext.Name) else ctxName
                    aNode, if exten = "s" then (sprintf "%s" ctx) else (sprintf "%s  ~  %s" ctx exten))
        )

    let rec ExtractEdges (ctxLst: Context list) edges =
        match ctxLst with
        |   [] ->  edges
        | head::tail ->
            ExtractEdges tail ((GetLocalEdges head.Extensions head.Name )::edges)

    let IsMatchStartNode desiredNode (vertix: (string * string)) = 
        fst vertix = desiredNode


    let rec DepthSearch inGraph (root: string) (outVertices: (string*string) list)=
        let desired = 
            match root.Split "  ~  " with 
            | [|fst|] -> fst
            | [|fst;snd|] when snd = "s" -> fst
            | _ -> root
        printfn "%s" desired
        let nodeVertices = 
            inGraph
            |> List.filter (IsMatchStartNode desired)
            |> List.where (fun vert -> not (List.contains vert outVertices))
        match nodeVertices with 
        | [] -> outVertices
        | _  -> nodeVertices 
                |> List.collect (fun vert -> DepthSearch inGraph (snd vert) (nodeVertices @ outVertices))

    let edges =
        ExtractEdges parsed []
        |> List.concat
        |> List.filter(fun mp -> not ((snd mp).Equals("  ~  ")) )
        |> List.distinct
        
    // for edge in edges do 
    //     printfn "%s - %s" (fst edge) (snd edge)

    let filteredEdges = 
        DepthSearch edges "from-erth_550  ~  553" []
        |> List.distinct
        

    printfn "%d" edges.Length
    printfn "%d" filteredEdges.Length
    for edge in filteredEdges do
        printfn "%s    -    %s" (fst edge) (snd edge)

// #r "/home/LDAP/preslerks/.nuget/packages/xplot.d3/1.0.0/lib/netstandard2.0/XPlot.D3.dll"
// open XPlot.D3

    let chrt =
        filteredEdges
        |> Chart.ForceLayout
        |> Chart.WithHeight 2800
        |> Chart.WithWidth 2800
        |> Chart.WithGravity 0.4
        |> Chart.WithCharge -500.0
        |> Chart.WithEdgeOptions (fun e ->
            let pr = e.From.Name.Split("  ~  "), e.To.Name
            match pr with
            | fst , _ when fst.Length > 1 && fst.[1].Equals("553")    -> { defaultEdgeOptions with Distance = 500.0 }
            // | _,"s" -> { defaultEdgeOptions with StrokeWidth = 4.5 }
            | _ -> {defaultEdgeOptions with Distance = 500.0})
        |> Chart.WithNodeOptions(fun n ->
            let spltd =  n.Name.Split("  ~  ")
            match spltd with
            | [|"from-erth_550"; "553"|] when spltd.Length = 2  -> 
                {defaultNodeOptions with 
                    RadiusScale=0.8;
                    Fill = {Red = 200uy; Green = 15uy; Blue=5uy}; 
                    Label = Some({Text = n.Name; StyleAttrs = []})}
            // | [|"from-protei"; _ |] when spltd.Length = 2 -> {defaultNodeOptions with RadiusScale=4.0;Fill = {Red = 200uy; Green = 150uy;           Blue=5uy}}
            | _ -> {defaultNodeOptions with RadiusScale=0.2; Fill = {Red = 0uy;Green = 195uy; Blue=10uy};Label = Some({Text = n.Name; StyleAttrs = []})})

    chrt.Show()

// let edges =
//     [   "A", "B"
//         "B", "C"
//         "C", "A"
//         "A", "D"
//         "B", "E"];;

// let chrt =
//     edges
//     |> Chart.ForceLayout
//     |> Chart.WithHeight 300
//     |> Chart.WithWidth 400
//     |> Chart.WithGravity 0.5
//     |> Chart.WithCharge -2000.0
//     |> Chart.WithEdgeOptions (fun e ->
//         let pr = e.From.Name, e.To.Name
//         match pr with
//         | "A","B" -> { defaultEdgeOptions with Distance = 200.0 }
//         | "A","D" -> { defaultEdgeOptions with StrokeWidth = 4.5 }
//         | _ -> {defaultEdgeOptions with Distance = 100.0})
//     |> Chart.WithNodeOptions(fun n ->
//         match n.Name with
//         | "A" -> {defaultNodeOptions with Fill = {Red = 150uy; Green = 150uy;       Blue=195uy}}
//         | "B" -> {defaultNodeOptions with RadiusScale=1.5; Fill = {Red = 150uy;         Green = 195uy; Blue=150uy}}
//         | _ -> defaultNodeOptions);;


//     chrt.Show()

    0 // return an integer exit code