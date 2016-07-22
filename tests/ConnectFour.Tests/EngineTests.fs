module ConnectFour.Tests

open FSharpx.Collections
open FsUnitTyped
open NUnit.Framework

let initialState = 
    { status = Turn Black
      gameBoard = newGameBoard
      bitBoard = newBitBoard
      blackBoard = PlayerBoard(Black, newBitBoard)
      redBoard = PlayerBoard(Red, newBitBoard) }

[<Test>]
let ``swapTurn changes state.playerTurn to opposite player``() = 
    let swapped = swapTurn initialState
    swapped.status |> shouldEqual <| Turn Red
    let reSwapped = swapTurn swapped
    reSwapped.status |> shouldEqual <| Turn Black

[<Test>]
let ``hasFreeSpace for empty column``() = 
    let column = Column PersistentVector.empty
    hasFreeSpace column |> shouldEqual true

[<Test>]
let ``hasFreeSpace for full column``() = 
    let column = Column(PersistentVector.init rows (fun _ -> Black))
    hasFreeSpace column |> shouldEqual false

[<Test>]
let ``hasFreeSpace for column with empty spaces``() = 
    let column = Column(PersistentVector.init (rows - 1) (fun _ -> Black))
    hasFreeSpace column |> shouldEqual true

[<Test>]
let ``isValid column number``() = 
    isValid 0 |> shouldEqual false
    isValid 1 |> shouldEqual true
    isValid 7 |> shouldEqual true
    isValid 8 |> shouldEqual false

[<Test>]
let ``getColumn for valid column number``() = 
    let columns = PersistentVector.init columns (fun _ -> newColumn)
    getColumn 1 columns
    |> shouldEqual
    <| Ok(newColumn)

[<Test>]
let ``getColumn for invalid column number``() = 
    let columns = PersistentVector.init columns (fun _ -> newColumn)
    getColumn 0 columns
    |> shouldEqual
    <| Error InvalidColumn

[<Test>]
let ``addPiece to non-full column``() = 
    let column = Column PersistentVector.empty
    addPiece Black column
    |> shouldEqual
    <| Ok(Column(PersistentVector.singleton Black))

[<Test>]
let ``addPiece to full column``() = 
    let column = Column <| PersistentVector.init rows (fun _ -> Black)
    addPiece Black column
    |> shouldEqual
    <| Error FullColumn

[<Test>]
let ``dropPiece adds piece to column``() = 
    let colNumber = 1
    let newState = dropPiece initialState colNumber Black |> Choice.get
    let { gameBoard = (GameBoard columns) } = newState
    PersistentVector.nth (colNumber - 1) columns
    |> shouldEqual
    <| Column(PersistentVector.singleton Black)

[<Test>]
let ``dropPiece returns Error for full column``() = 
    let colNumber = 1
    let { gameBoard = (GameBoard columns) } = initialState
    let fullColumn = Column <| PersistentVector.init rows (fun _ -> Black)
    let fullColumnState = {initialState with gameBoard = GameBoard (PersistentVector.update (colNumber - 1) fullColumn columns)}
    dropPiece fullColumnState colNumber Black
    |> shouldEqual
    <| Error FullColumn

[<Test>]
let ``isWinningBoard is false for empty board`` () =
    isWinningBoard newBitBoard |> shouldEqual false

[<Test>]
let ``isWinningBoard is true for full board`` () =
    isWinningBoard fullBitBoard |> shouldEqual true