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
Node.require.Invoke("core-js") |> ignore

type Action =
    | ColumnClick of int

type Model = { gameState: GameState }
let newModel = { gameState = newGameState Red }

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

let board columns =
    let startIndex = rows - 1
    div [attribute "class" "board"] 
        (List.map (fun rowIndex -> row (rowFromColumns columns rowIndex)) [startIndex..(-1)..0])

let colorString color =
    match color with
    | Red -> "Red"
    | Black -> "Black"
    | _ -> failwith "Invalid color"

let statusView status =
    let message = match status with
                  | Turn color -> sprintf "%s Player's Turn" (colorString color)
                  | Tie -> "Draw Game"
                  | Winner color -> sprintf "%s Player Wins!" (colorString color)
    div [attribute "class" "status"]
        [h3 [] [text message]]

let view {gameState = gameState} =
    let {gameBoard = (GameBoard columns); status = status} = gameState
    div [attribute "class" "container"]
        [
            statusView status
            board columns
        ]

let update model action = 
    let {gameState = gameState} = model
    match action with
    | ColumnClick colNumber -> 
        match (dropPiece gameState colNumber) with
        | (Ok updatedState) -> {model with gameState = updatedState}
        | Error err -> 
            printfn "Error: %A" err
            model
    |> (fun m -> m, [], [])

App.createApp {Model = newModel; View = view; Update = update}
|> App.withStartNode "#app"
|> App.start Virtualdom.renderer
|> ignore