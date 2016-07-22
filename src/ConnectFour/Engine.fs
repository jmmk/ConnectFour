module ConnectFour

open FSharpx.Collections

let Ok = Choice1Of2
let Error = Choice2Of2
let (>>=) x y = Choice.bind y x
let (<!>) x y = Choice.mapError y x

type Color = 
    | Black
    | Red

/// Single column of the board
/// Pieces can be "dropped" into the column, filling it from the "bottom" up
type Column = 
    | Column of PersistentVector<Color>

/// Array of columns representing the entire board
/// Used for validating moves and displaying the board state
type GameBoard = 
    | GameBoard of PersistentVector<Column>

/// Bit board used for scoring
type BitBoard = 
    | BitBoard of int64

/// Number of rows in the board
let rows = 6

/// Number of columns in the board
let columns = 7

let newColumn = Column PersistentVector.empty
let newGameBoard = GameBoard(PersistentVector.init columns (fun _ -> newColumn))

/// Set the bit of integer i at position p to 1
let bitSet i p =
    i ||| (1 <<< p)

/// A BitBoard is a 64-bit integer whose bits represent board spaces.
/// The bit position represented by each board space is as follows:
/// | 5 12 19 26 33 40 47 |
/// | 4 11 18 25 32 39 46 |
/// | 3 10 17 24 31 38 45 |
/// | 2  9 16 23 30 37 44 |
/// | 1  8 15 22 29 36 43 |
/// | 0  7 14 21 28 35 42 |
/// -----------------------
///
/// An "empty" board has 0's in each pown
/// When a piece is played into a position, its bit becomes 1
///
/// If we create a list of these values:
/// let boardValues = [0;  1;  2;  3;  4;  5
///                    7;  8;  9;  10; 11; 12
///                    14; 15; 16; 17; 18; 19
///                    21; 22; 23; 24; 25; 26
///                    28; 29; 30; 31; 32; 33
///                    35; 36; 37; 38; 39; 40
///                    42; 43; 44; 45; 46; 47]
/// 
/// We can compute the value of a full bit-board with the following:
/// List.fold bitSet 0L boardValues
/// which gives us 279258638311359L
let fullBitBoard = BitBoard 279258638311359L
/// an empty bitboard is simply 0
let newBitBoard = BitBoard 0L

type PlayerBoard = 
    | PlayerBoard of Color * BitBoard

type GameStatus = 
    | Winner of Color
    | Turn of Color
    | Draw

type GameState = 
    { status : GameStatus
      gameBoard : GameBoard
      bitBoard : BitBoard
      blackBoard : PlayerBoard
      redBoard : PlayerBoard }

let newGameState piece = 
    { status = Turn piece
      gameBoard = newGameBoard
      bitBoard = newBitBoard
      blackBoard = PlayerBoard(Red, newBitBoard)
      redBoard = PlayerBoard(Black, newBitBoard) }

let swapTurn state = 
    match state.status with
    | Turn Black -> { state with status = Turn Red }
    | Turn Red -> { state with status = Turn Black }
    | _ -> state

let hasFreeSpace (Column spaces) = PersistentVector.length spaces < rows
let isValid column = column > 0 && column <= columns

type EngineError = 
    | FullColumn
    | InvalidColumn

let getColumn colNumber columns = 
    if isValid colNumber then Ok <| PersistentVector.nth (colNumber - 1) columns
    else Error InvalidColumn

let addPiece piece column = 
    let (Column spaces) = column
    if hasFreeSpace column then Ok <| Column(PersistentVector.conj piece spaces)
    else Error FullColumn

let dropPiece state colNumber piece = 
    let { gameBoard = (GameBoard columns) } = state
    getColumn colNumber columns
    >>= addPiece piece
    |> Choice.map (fun col -> PersistentVector.update (colNumber - 1) col columns)
    |> Choice.map (fun cols -> { state with gameBoard = GameBoard cols })

/// algorithm found here: http://stackoverflow.com/a/4261803
let isWinningBoard (BitBoard bitBoard) = 
    let y = bitBoard &&& (bitBoard >>> 6)
    let z = bitBoard &&& (bitBoard >>> 7)
    let w = bitBoard &&& (bitBoard >>> 8)
    let x = bitBoard &&& (bitBoard >>> 1)
    (y &&& (y >>> 12)) ||| (z &&& (z >>> 14)) ||| (w &&& (w >>> 16)) ||| (x &&& (x >>> 2)) |> (<>) 0L