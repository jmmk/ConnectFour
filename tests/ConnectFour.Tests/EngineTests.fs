module ConnectFour.Tests

open FsUnitTyped
open NUnit.Framework

[<Test>]
let ``swapTurn changes state.playerTurn to opposite player``() = 
    let initial = { playerTurn = Black }
    let swapped = swapTurn initial
    swapped.playerTurn |> shouldEqual Red

    let reSwapped = swapTurn swapped
    reSwapped.playerTurn |> shouldEqual Black