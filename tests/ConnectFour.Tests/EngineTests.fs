module ConnectFour.Tests

open FsUnitTyped
open NUnit.Framework

[<Test>]
let ``swapTurn for Black player changes state to Red player``() = 
    let initial = { playerTurn = Black }
    let expected = { playerTurn = Red }
    ConnectFour.swapTurn initial |> shouldEqual expected

[<Test>]
let ``swapTurn for Red player changes state to Black player``() = 
    let initial = { playerTurn = Red }
    let expected = { playerTurn = Black }
    ConnectFour.swapTurn initial |> shouldEqual expected
