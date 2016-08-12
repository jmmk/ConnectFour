module UI

open ConnectFour
open Fable.Core
open Fable.Helpers
open Fable.Helpers.Virtualdom
open Fable.Helpers.Virtualdom.Html
open Fable.Import
open Fable.Import.long
open Fable.Import.mori

// Evaluate polyfill code before anything else
Node.require.Invoke("core-js") |> ignore

// Include CSS
Node.require.Invoke("app.scss") |> ignore

type Action = 
    | ColumnClick of int
    | NewGameClick

type Model = 
    { gameState : GameState }

let newModel = { gameState = newGameState Red }

let cell colIndex color = 
    let className = 
        match color with
        | Red -> "red"
        | Black -> "black"
        | Empty -> "empty"
    div [ attribute "class" "cell"
          Events.onMouseClick (fun _ -> ColumnClick(colIndex + 1)) ] 
        [ div [ attribute "class" (sprintf "piece %s" className) ] [] ]

let row colors = 
    let endIndex = columns - 1
    div [ attribute "class" "row" ] 
        (List.map (fun colIndex -> cell colIndex (Vector.nth (colors, colIndex))) [ 0..endIndex ])

let rowFromColumns columns rowIndex = Vector.map ((fun (Column col) -> Vector.nth (col, rowIndex, Empty)), columns)

let board columns = 
    let startIndex = rows - 1
    div [attribute "class" "board-container"]
        [div [ attribute "class" "board" ] 
             (List.map (fun rowIndex -> row (rowFromColumns columns rowIndex)) [ startIndex..(-1)..0 ])
        ]

let boardControls =
    div [attribute "class" "board-controls"]
        [button [attribute "class" "new-game-button"
                 onMouseClick (fun _ -> NewGameClick)] 
                [text "New Game!"]]

let colorString color = 
    match color with
    | Red -> "Red"
    | Black -> "Black"
    | _ -> failwith "Invalid color"

let playerText color = 
    let colorText = 
        if color = Red then "Red"
        else "Black"
    
    let className = 
        if color = Red then "red"
        else "black"
    
    span [ attribute "class" (sprintf "player-text %s" className) ] [ text colorText ]

let statusMessage message = span [] [ text message ]

let statusView status = 
    let children = 
        match status with
        | Turn color -> 
            [ playerText color
              statusMessage " Player's Turn" ]
        | Tie -> [ statusMessage "Draw Game" ]
        | Winner color -> 
            [ playerText color
              statusMessage " Player Wins!" ]
    div [ attribute "class" "status" ] [ h3 [ attribute "class" "status-text" ] children ]

let view { gameState = gameState } = 
    let { gameBoard = (GameBoard columns); status = status } = gameState
    div [ attribute "class" "game-container" ] [ statusView status
                                                 board columns
                                                 boardControls ]

let update model action = 
    let { gameState = gameState } = model
    match action with
    | ColumnClick colNumber -> 
        match (dropPiece gameState colNumber) with
        | (Ok updatedState) -> { model with gameState = updatedState }
        | Error err -> 
            printfn "Error: %A" err
            model
    | NewGameClick ->
        newModel
    |> (fun m -> m, [])

App.createApp newModel view update
|> App.withStartNodeSelector "#app"
|> App.start Virtualdom.renderer
|> ignore
