open System
open Aardvark.Base
open Aardvark.Rendering
open Aardvark.Application.Slim
open FShade

[<LocalSize(X = 64)>]
let add1 (n: float32[]) =
    compute {
        let id = getGlobalId().X
        n.[id] <- n.[id] + 1.0f
    }

[<EntryPoint; STAThread>]
let main argv =
    Aardvark.Init()

    use app = new OpenGlApplication()

    let runtime = app.Runtime :> IRuntime

    let testArr = [| 1.0f .. 100.0f |]
    let input = runtime.CreateBuffer<float32>(testArr)

    let targetWriteShader = runtime.CreateComputeShader add1
    let targetWrite = runtime.NewInputBinding(targetWriteShader)

    targetWrite.["n"] <- input

    targetWrite.Flush()

    let ceilDiv (v: int) (d: int) = if v % d = 0 then v / d else 1 + v / d

    let mk =
        [ ComputeCommand.Bind(targetWriteShader)
          ComputeCommand.SetInput targetWrite
          ComputeCommand.Dispatch(ceilDiv (int input.Count) 64) ]

    let program = runtime.Compile mk

    program.Run()

    let result = input.Download()

    printfn "Result 1: %A" result

    for _i in 1..99 do
        program.Run()

    let result = input.Download()

    printfn "Result 100: %A" result


    0 // return code
