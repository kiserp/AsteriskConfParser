// #nowarn "211"
// #I "/home/LDAP/preslerks/.nuget/packages/rprovider/1.1.22/"
// #I "/home/LDAP/preslerks/.nuget/packages/r.net.fsharp/1.7.0/"
// #I "/home/LDAP/preslerks/.nuget/packages/r.net.fsharp/1.7.0/lib/net40/"
// #I "/home/LDAP/preslerks/.nuget/packages/r.net/1.7.0/lib/net40/"
// #load "RDotNet.FSharp.fsx"
// #load "RProvider.fsx"

// open RProvider
// open RProvider.graphics
// open RProvider.grDevices
// open RProvider.datasets

// R.mean([1;2;3;4])

// R.x11()

#r "/home/LDAP/preslerks/.nuget/packages/xplot.d3/1.0.0/lib/netstandard2.0/XPlot.D3.dll"
#r "/home/LDAP/preslerks/.nuget/packages/netstandard.library.netframework/2.0.0-preview2-25405-01/build/net47/lib/System.Runtime.InteropServices.RuntimeInformation.dll"
open XPlot.D3
let edges = ["A", "B"
             "B", "C"
             "C", "A"
             "A", "D"
             "B", "E"]

let chrt =
    edges
    |> Chart.ForceLayout
    |> Chart.WithHeight 300
    |> Chart.WithWidth 400
    |> Chart.WithGravity 0.5
    |> Chart.WithCharge -2000.0
    |> Chart.WithEdgeOptions (fun e ->
        let pr = e.From.Name, e.To.Name
        match pr with
        | "A","B" -> { defaultEdgeOptions with Distance = 200.0 }
        | "A","D" -> { defaultEdgeOptions with StrokeWidth = 4.5 }
        | _ -> {defaultEdgeOptions with Distance = 100.0})
    |> Chart.WithNodeOptions(fun n ->
        match n.Name with
        | "A" -> {defaultNodeOptions with Fill = {Red = 150uy; Green = 150uy;       Blue=195uy}}
        | "B" -> {defaultNodeOptions with RadiusScale=1.5; Fill = {Red = 150uy;         Green = 195uy; Blue=150uy}}
        | _ -> defaultNodeOptions)

chrt.Show()
