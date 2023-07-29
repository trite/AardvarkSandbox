module ContinuousSpatialAutomata

open System
open Aardvark.Base
open Aardvark.Rendering
open Aardvark.Application.Slim
open FShade

// Needs to be fed a `kernelRowLength` that is odd
// TODO: Enforce this on the F# side of things
[<LocalSize(X = 64)>]
let csaSimulation
    (cells: float32[])
    (rowLength: int)
    (kernel: float32[])
    (kernelRowLength: int)
    (kernelTotal: float32)
    =
    compute {

        let id = getGlobalId().X

        let mutable sum = 0.0f

        for y in 0 .. kernelRowLength - 1 do
            for x in 0 .. kernelRowLength - 1 do
                if (x >= 0 && x < rowLength && y >= 0 && y < rowLength) then
                    let cell = cells.[id + x + y * rowLength]
                    let kernelCell = kernel.[x + y * kernelRowLength]

                    sum <- sum + cell * kernelCell

        let weightedSum = sum / kernelTotal

        let mainCalc = (1f / (0.9f + (((weightedSum - 0.375f) / 0.05f) ** 2f))) - 0.1f

        cells[id] <- min 1.0f (max 0.0f mainCalc)
    }

let printGrid (cells: float32[]) (rowLength: int) =
    let printCell (cell: float32) = sprintf "%4f" cell

    let limit = min (rowLength - 1) 10

    for y in 0..limit do
        for x in 0..limit do
            printf "%s " (printCell cells.[y * rowLength + x])

        printfn ""

let run () =
    Aardvark.Init()

    use app = new OpenGlApplication()

    let runtime = app.Runtime :> IRuntime

    let rowLength = 100

    let testArr =
        Array.init (rowLength * rowLength) (fun i ->
            let isCell (i: int) (x: int) (y: int) = i = x + y * rowLength

            if isCell i 4 4 || isCell i 5 5 || isCell i 5 6 || isCell i 4 6 || isCell i 3 6 then
                1f
            else
                0f)

    let kernelRowLength = 3

    let kernel = [| 1.0f; 1.0f; 1.0f; 1.0f; 0.0f; 1.0f; 1.0f; 1.0f; 1.0f |]

    let input = runtime.CreateBuffer<float32>(testArr)
    let kernelInput = runtime.CreateBuffer<float32>(kernel)

    let targetWriteShader = runtime.CreateComputeShader csaSimulation
    let targetWrite = runtime.NewInputBinding(targetWriteShader)

    let kernelTotal = kernel |> Array.sum

    targetWrite.["cells"] <- input
    targetWrite.["rowLength"] <- rowLength

    targetWrite.["kernel"] <- kernelInput
    targetWrite.["kernelRowLength"] <- kernelRowLength

    targetWrite.["kernelTotal"] <- kernelTotal

    targetWrite.Flush()

    let ceilDiv (v: int) (d: int) = if v % d = 0 then v / d else 1 + v / d

    let mk =
        [ ComputeCommand.Bind(targetWriteShader)
          ComputeCommand.SetInput targetWrite
          ComputeCommand.Dispatch(ceilDiv (int input.Count) 64) ]

    let program = runtime.Compile mk

    printfn "Initial:"
    printGrid testArr rowLength

    program.Run()

    let result = input.Download()

    printfn "Result 1:"
    printGrid result rowLength

    program.Run()

    let result = input.Download()

    printfn "Result 2:"
    printGrid result rowLength

    program.Run()

    let result = input.Download()

    printfn "Result 3:"
    printGrid result rowLength
