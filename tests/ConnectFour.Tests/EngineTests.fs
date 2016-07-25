module ConnectFour.Tests

open FSharpx.Collections
open FsUnitTyped
open NUnit.Framework

let inline (^<|) f a = f a

let initialState = 
    { status = Turn Black
      gameBoard = newGameBoard
      bitBoard = newBitBoard
      playerBoards = 
          Map.ofList [ (Red, newBitBoard)
                       (Black, newBitBoard) ] }

[<Test>]
let ``swapTurn changes Turn status to opposite player``() = 
    swapTurn (Turn Black) |> shouldEqual ^<| Turn Red
    swapTurn (Turn Red) |> shouldEqual ^<| Turn Black

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
    getColumn 1 columns |> shouldEqual ^<| Ok(newColumn)

[<Test>]
let ``getColumn for invalid column number``() = 
    let columns = PersistentVector.init columns (fun _ -> newColumn)
    getColumn 0 columns |> shouldEqual ^<| Error InvalidColumn

[<Test>]
let ``addPiece to non-full column``() = 
    let column = Column PersistentVector.empty
    addPiece Black column |> shouldEqual ^<| Ok(Column(PersistentVector.singleton Black))

[<Test>]
let ``addPiece to full column``() = 
    let column = Column <| PersistentVector.init rows (fun _ -> Black)
    addPiece Black column |> shouldEqual ^<| Error FullColumn

[<Test>]
let ``dropPiece adds piece to column, bitboard, and playerboard``() = 
    let colNumber = 1
    let newState = dropPiece initialState colNumber Black |> Choice.get
    let { gameBoard = (GameBoard columns); playerBoards = playerBoards; bitBoard = (BitBoard bitBoard) } = newState
    PersistentVector.nth (colNumber - 1) columns |> shouldEqual ^<| Column(PersistentVector.singleton Black)
    bitBoard |> shouldEqual 1L
    Map.find Black playerBoards |> shouldEqual (BitBoard 1L)

[<Test>]
let ``dropPiece returns Error for full column``() = 
    let colNumber = 1
    let { gameBoard = (GameBoard columns) } = initialState
    let fullColumn = Column <| PersistentVector.init rows (fun _ -> Black)
    let fullColumnState = 
        { initialState with gameBoard = GameBoard(PersistentVector.update (colNumber - 1) fullColumn columns) }
    dropPiece fullColumnState colNumber Black |> shouldEqual ^<| Error FullColumn

[<Test>]
let ``isWinningBoard is false for empty board``() = isWinningBoard newBitBoard |> shouldEqual false

[<Test>]
let ``isWinningBoard is true for full board``() = isWinningBoard fullBitBoard |> shouldEqual true

let bitBoardTotal pieces = List.fold bitSet 0L pieces

[<Test>]
let ``isWinningBoard vertical``() = 
    // Four in a row
    let bitBoard = BitBoard <| bitBoardTotal [ 0; 1; 2; 3 ]
    isWinningBoard bitBoard |> shouldEqual true
    // Three in a row
    let bitBoard = BitBoard <| bitBoardTotal [ 0; 1; 2 ]
    isWinningBoard bitBoard |> shouldEqual false

[<Test>]
let ``isWinningBoard horizontal``() = 
    // Four in a row
    let bitBoard = BitBoard <| bitBoardTotal [ 0; 7; 14; 21 ]
    isWinningBoard bitBoard |> shouldEqual true
    let bitBoard = BitBoard <| bitBoardTotal [ 26; 33; 40; 47 ]
    isWinningBoard bitBoard |> shouldEqual true
    // Three in a row
    let bitBoard = BitBoard <| bitBoardTotal [ 0; 7; 14 ]
    isWinningBoard bitBoard |> shouldEqual false
    let bitBoard = BitBoard <| bitBoardTotal [ 26; 33; 40 ]
    isWinningBoard bitBoard |> shouldEqual false

[<Test>]
let ``isWinningBoard diagonal``() = 
    // Four in a row
    let bitBoard = BitBoard <| bitBoardTotal [ 0; 8; 16; 24 ]
    isWinningBoard bitBoard |> shouldEqual true
    // Three in a row
    let bitBoard = BitBoard <| bitBoardTotal [ 0; 8; 16 ]
    isWinningBoard bitBoard |> shouldEqual false

[<Test>]
let ``end to end``() = 
    let endState = 
        initialState
        |> (fun state -> dropPiece state 1 Black |> Choice.get)
        |> (fun state -> dropPiece state 2 Red |> Choice.get)
        |> (fun state -> dropPiece state 1 Black |> Choice.get)
        |> (fun state -> dropPiece state 2 Red |> Choice.get)
        |> (fun state -> dropPiece state 1 Black |> Choice.get)
        |> (fun state -> dropPiece state 2 Red |> Choice.get)
        |> (fun state -> dropPiece state 1 Black |> Choice.get)
    endState
    |> (fun { playerBoards = playerBoards } -> Map.find Black playerBoards)
    |> isWinningBoard
    |> shouldEqual true
    let { status = status } = endState
    status |> shouldEqual ^<| Winner Black
