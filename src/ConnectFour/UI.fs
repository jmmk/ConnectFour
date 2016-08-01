module UI

open ConnectFour

open Fable.Core
open Fable.Import

open Fable.Helpers
open Fable.Helpers.Virtualdom
open Fable.Helpers.Virtualdom.Html

// Evaluate polyfill code before anything else
Node.require.Invoke("core-js") |> ignore

let cell row column =
    let isEmpty = if (row + column) % 3 = 0 then true else false
    let isRed = if (row + column) % 3 = 1 then true else false
    div [attribute "class" "cell"] 
        [div [attribute "class" (if isEmpty then "piece empty" elif isRed then "piece red" else "piece black")] []]

let row i = 
    div [attribute "class" "row"]
        (List.map (cell i) [0..6])

let view model =
    div [attribute "class" "board"] 
        (List.map row [5..-1..0])

let update model action = (model, [], [])

let board = newGameBoard

printfn "%A" board

App.createApp {Model = 0; View = view; Update = update}
|> App.withStartNode "#app"
|> App.start Virtualdom.renderer
|> ignore