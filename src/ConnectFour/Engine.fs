namespace ConnectFour

module Engine = 
    open ConnectFour.Result
    open Fable.Import.long
    open Fable.Import.mori
    
    type Color = 
        | Black
        | Red
        | Empty
    
    /// Single column of the board
    /// Pieces can be "dropped" into the column, filling it from the "bottom" up
    type Column = 
        | Column of Vector<Color>
    
    /// Array of columns representing the entire board
    /// Used for validating moves and displaying the board state
    type GameBoard = 
        | GameBoard of Vector<Column>
    
    /// Bit board used for scoring
    type BitBoard = 
        | BitBoard of Long
    
    /// Number of rows in the board
    let rows = 6
    
    /// Number of columns in the board
    let columns = 7
    
    let newColumn = Column <| Vector.vector()
    let newGameBoard = GameBoard(Vector.vector (Array.init columns (fun _ -> newColumn)))
    
    /// Set the bit at position i to 1
    let bitSet (bitBoard : Long) (i : int) = bitBoard.``or`` (Long.ONE.shiftLeft (i))
    
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
    let fullBitBoard = Long.fromNumber (279258638311359L)
    
    /// an empty bitboard is simply 0
    let newBitBoard = BitBoard Long.ZERO
    
    type GameStatus = 
        | Winner of Color
        | Turn of Color
        | Tie
    
    type GameState = 
        { status : GameStatus
          gameBoard : GameBoard
          playerBoards : Map<Color, BitBoard>
          bitBoard : BitBoard }
    
    let newGameState piece = 
        { status = Turn piece
          gameBoard = newGameBoard
          bitBoard = newBitBoard
          playerBoards = 
              Map.ofList [ (Red, newBitBoard)
                           (Black, newBitBoard) ] }
    
    let swapTurn status = 
        match status with
        | Turn Black -> Turn Red
        | Turn Red -> Turn Black
        | _ -> status
    
    let hasFreeSpace (Column spaces) = Vector.count (spaces) < rows
    let isValid column = column > 0 && column <= columns
    
    /// algorithm found here: http://stackoverflow.com/a/4261803
    let isWinningBoard (BitBoard bitBoard) = 
        let y = bitBoard.``and`` (bitBoard.shiftRight (6))
        let z = bitBoard.``and`` (bitBoard.shiftRight (7))
        let w = bitBoard.``and`` (bitBoard.shiftRight (8))
        let x = bitBoard.``and`` (bitBoard.shiftRight (1))
        (y.``and`` (y.shiftRight (12))).``or``(z.``and`` (z.shiftRight (14))).``or``(w.``and`` (w.shiftRight (16)))
            .``or``(x.``and`` (x.shiftRight (2))) |> (fun long -> not (long.isZero()))
    
    let isDrawBoard (BitBoard bitBoard) = bitBoard.equals (fullBitBoard)
    
    type EngineError = 
        | FullColumn
        | InvalidColumn
        | IllegalOperation
    
    let getColumn colNumber columns = 
        if isValid colNumber then Ok <| Vector.nth (columns, (colNumber - 1))
        else Error InvalidColumn
    
    let addPiece piece column = 
        let (Column spaces) = column
        if hasFreeSpace column then Ok <| Column(Vector.conj (spaces, piece))
        else Error FullColumn
    
    let updateGameBoard state colNumber piece = 
        let { gameBoard = (GameBoard columns) } = state
        getColumn colNumber columns
        |> bind (fun col -> addPiece piece col)
        |> map (fun col -> GameBoard(Vector.assoc (columns, (colNumber - 1), col)))
    
    let addBit (BitBoard bitBoard) colNumber col = 
        let x = colNumber - 1
        let y = Vector.count (col) - 1
        BitBoard(bitSet bitBoard ((x * 7) + y))
    
    let updateBitBoard state colNumber = 
        let { bitBoard = bitBoard; gameBoard = (GameBoard columns) } = state
        let (Column column) = Vector.nth (columns, (colNumber - 1))
        addBit bitBoard colNumber column
    
    let updatePlayerBoards state colNumber piece = 
        let { playerBoards = playerBoards; gameBoard = (GameBoard columns) } = state
        let (Column column) = Vector.nth (columns, (colNumber - 1))
        let playerBoard = Map.find piece playerBoards
        Map.add piece (addBit playerBoard colNumber column) playerBoards
    
    let updateStatus state colNumber piece = 
        let { bitBoard = bitBoard; status = status; playerBoards = playerBoards } = state
        let playerBoard = Map.find piece playerBoards
        match status with
        | Turn piece -> 
            if isWinningBoard playerBoard then Winner piece
            elif isDrawBoard bitBoard then Tie
            else swapTurn status
        | _ -> status
    
    let dropPiece state colNumber = 
        let { status = status } = state
        match status with
        | Turn piece -> 
            updateGameBoard state colNumber piece
            |> map (fun gameBoard -> { state with gameBoard = gameBoard })
            |> map (fun state -> { state with bitBoard = updateBitBoard state colNumber })
            |> map (fun state -> { state with playerBoards = updatePlayerBoards state colNumber piece })
            |> map (fun state -> { state with status = updateStatus state colNumber piece })
        | _ -> Ok state
