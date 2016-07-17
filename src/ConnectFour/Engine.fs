module ConnectFour

type Player = 
    | Black
    | Red

type State = 
    { playerTurn : Player }

let swapTurn state = 
    match state.playerTurn with
    | Black -> { state with playerTurn = Red }
    | Red -> { state with playerTurn = Black }
