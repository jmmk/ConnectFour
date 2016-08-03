module ConnectFour.Tests

open NUnit.Framework

open Fable.Import.long
open Fable.Import.mori

let inline (^<|) f a = f a

let shouldEqual (expected: 'T) (actual: 'T) =
    Assert.AreEqual(true, (expected = actual), sprintf "Expected: %A\nActual: %A" expected actual)

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
    let column = newColumn
    hasFreeSpace column |> shouldEqual true

[<Test>]
let ``hasFreeSpace for full column``() = 
    let column = Column(Vector.vector(Array.init columns (fun _ -> Black)))
    hasFreeSpace column |> shouldEqual false

[<Test>]
let ``hasFreeSpace for column with empty spaces``() = 
    let column = Column(Vector.vector(Array.init (rows - 1) (fun _ -> Black)))
    hasFreeSpace column |> shouldEqual true

[<Test>]
let ``isValid column number``() = 
    isValid 0 |> shouldEqual false
    isValid 1 |> shouldEqual true
    isValid 7 |> shouldEqual true
    isValid 8 |> shouldEqual false

[<Test>]
let ``getColumn for valid column number``() = 
    let columns = Vector.vector(Array.init columns (fun _ -> newColumn))
    getColumn 1 columns |> shouldEqual ^<| Ok(newColumn)

[<Test>]
let ``getColumn for invalid column number``() = 
    let columns = Vector.vector(Array.init columns (fun _ -> newColumn))
    getColumn 0 columns |> shouldEqual ^<| Error InvalidColumn

[<Test>]
let ``addPiece to non-full column``() = 
    let column = newColumn
    let updatedColumn = addPiece Black column 
    let vec = match updatedColumn with
                 | Ok (Column vec) -> vec
                 | _ -> failwith "Incorrect response"

    Vector.nth(vec, 0)
    |> shouldEqual 
    ^<| Black

[<Test>]
let ``addPiece to full column``() = 
    let column = Column <| Vector.vector(Array.init rows (fun _ -> Black))
    addPiece Black column |> shouldEqual ^<| Error FullColumn

[<Test>]
let ``dropPiece adds piece to column, bitboard, and playerboard``() = 
    let colNumber = 1
    let newState = dropPiece initialState colNumber |> get
    let { gameBoard = (GameBoard columns); playerBoards = playerBoards; bitBoard = (BitBoard bitBoard) } = newState
    let (Column vec) = Vector.nth(columns, (colNumber - 1)) 
    Vector.nth(vec, 0) |> shouldEqual ^<| Black
    true |> shouldEqual ^<| bitBoard.equals(Long.ONE)
    let (BitBoard playerBoard) = Map.find Black playerBoards 
    true |> shouldEqual ^<| playerBoard.equals(Long.ONE)

[<Test>]
let ``dropPiece returns Error for full column``() = 
    let colNumber = 1
    let { gameBoard = (GameBoard columns) } = initialState
    let fullColumn = Column <| Vector.vector(Array.init rows (fun _ -> Black))
    let fullColumnState = 
        { initialState with gameBoard = GameBoard(Vector.assoc(columns, (colNumber - 1), fullColumn)) }
    dropPiece fullColumnState colNumber |> shouldEqual ^<| Error FullColumn

[<Test>]
let ``isWinningBoard is false for empty board``() = isWinningBoard newBitBoard |> shouldEqual false

[<Test>]
let ``isWinningBoard is true for full board``() = isWinningBoard (BitBoard fullBitBoard) |> shouldEqual true

let bitBoardTotal pieces = List.fold bitSet Long.ZERO pieces

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
        |> (fun state -> dropPiece state 1 |> get)
        |> (fun state -> dropPiece state 2 |> get)
        |> (fun state -> dropPiece state 1 |> get)
        |> (fun state -> dropPiece state 2 |> get)
        |> (fun state -> dropPiece state 1 |> get)
        |> (fun state -> dropPiece state 2 |> get)
        |> (fun state -> dropPiece state 1 |> get)
    endState
    |> (fun { playerBoards = playerBoards } -> Map.find Black playerBoards)
    |> isWinningBoard
    |> shouldEqual true
    let { status = status } = endState
    status |> shouldEqual ^<| Winner Black
