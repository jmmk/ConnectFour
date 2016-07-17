module ConnectFour.Tests

open FsUnitTyped
open NUnit.Framework
open FSharpx.Collections

let initialState = {playerTurn = Black; gameBoard = newGameBoard; bitBoard = newBitBoard}

[<Test>]
let ``swapTurn changes state.playerTurn to opposite player``() = 
    let swapped = swapTurn initialState
    swapped.playerTurn |> shouldEqual Red

    let reSwapped = swapTurn swapped
    reSwapped.playerTurn |> shouldEqual Black

[<Test>]
let ``hasFreeSpace for empty column`` () =
    let column = Column PersistentVector.empty
    hasFreeSpace column |> shouldEqual true

[<Test>]
let ``hasFreeSpace for full column`` () =
    let column = Column (PersistentVector.init rows (fun i -> Black))
    hasFreeSpace column |> shouldEqual false

[<Test>]
let ``hasFreeSpace for column with empty spaces`` () =
    let column = Column (PersistentVector.init (rows - 1) (fun i -> Black))
    hasFreeSpace column |> shouldEqual true

[<Test>]
let ``move fills column from bottom`` () =
    let column = Column PersistentVector.empty
    move column Black |> shouldEqual (Column (PersistentVector.singleton Black))