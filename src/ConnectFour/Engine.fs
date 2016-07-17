module ConnectFour

open FSharpx.Collections

type Piece = 
    | Black
    | Red

/// Single column of the board
/// Pieces can be "dropped" into the column, filling it from the "bottom" up
type Column = Column of PersistentVector<Piece>

/// Array of columns representing the entire board
/// Used for validating moves and displaying the board state
type GameBoard = GameBoard of Column []

/// Bit board used for scoring
type BitBoard = BitBoard of int64

/// Number of rows in the board
let rows = 6
/// Number of columns in the board
let columns = 7

let newColumn = Column PersistentVector.empty
let newGameBoard = GameBoard (Array.create columns newColumn)

/// A full bit board has the following values at each space on the board:
///
/// [0;  1;  2;  3;  4;  5
///  7;  8;  9;  10; 11; 12
///  14; 15; 16; 17; 18; 19
///  21; 22; 23; 24; 25; 26
///  28; 29; 30; 31; 32; 33
///  35; 36; 37; 38; 39; 40
///  42; 43; 44; 45; 46; 47]
/// 
/// which can be represented by the 64-bit integer 279258638311359L
let fullBitBoard = BitBoard 279258638311359L

/// an empty bitboard is simply 0
let newBitBoard = BitBoard 0L

type GameState = 
    { playerTurn : Piece;
      gameBoard: GameBoard;
      bitBoard: BitBoard }

let swapTurn state = 
    match state.playerTurn with
    | Black -> { state with playerTurn = Red }
    | Red -> { state with playerTurn = Black }


let hasFreeSpace (Column spaces) = PersistentVector.length spaces < rows

let move (Column spaces) piece =
    Column (PersistentVector.conj piece spaces)