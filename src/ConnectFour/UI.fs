module UI

open ConnectFour

open Fable.Core
open Fable.Import
open Fable.Import.long
open Fable.Import.mori

open Fable.Helpers
open Fable.Helpers.Virtualdom
open Fable.Helpers.Virtualdom.Html

// Evaluate polyfill code before anything else
// Node.require.Invoke("core-js") |> ignore

type Action =
    | ColumnClick of int

let cell colIndex color =
    let className =
        match color with
        | Red -> "red"
        | Black -> "black"
        | Empty -> "empty"
    div [ attribute "class" "cell"
          Events.onMouseClick (fun _ -> ColumnClick (colIndex + 1))] 
        [div [attribute "class" (sprintf "piece %s" className)] []]

let row colors = 
    let endIndex = columns - 1
    div [attribute "class" "row"]
        (List.map (fun colIndex -> cell colIndex (Vector.nth(colors, colIndex))) [0..endIndex])

let rowFromColumns columns rowIndex =
    Vector.map((fun (Column col) -> Vector.nth(col, rowIndex, Empty)), columns)

let view model =
    let {gameBoard = (GameBoard columns)} = model
    let startIndex = rows - 1
    div [attribute "class" "board"] 
        (List.map (fun rowIndex -> row (rowFromColumns columns rowIndex)) [startIndex..(-1)..0])

let update model action = 
    match action with
    | ColumnClick colNumber -> 
        match (dropPiece model colNumber) with
        | (Ok updatedState) -> updatedState
        | Error err -> 
            printfn "Error: %A" err
            model
    |> (fun m -> m, [], [])

App.createApp {Model = newGameState Red; View = view; Update = update}
|> App.withStartNode "#app"
|> App.start Virtualdom.renderer
|> ignore