open System
open Aardvark.Base
open Aardvark.Rendering
open Aardvark.Application.Slim
open FShade

[<EntryPoint; STAThread>]
let main _argv =
    // CellularAutomata.run ()
    ContinuousSpatialAutomata.run ()

    0 // return code
