namespace ConnectFour

/// Result shim until F# 4.1
module Result = 
    let (|Ok|Error|) choice = 
        match choice with
        | Choice1Of2 a -> Ok a
        | Choice2Of2 b -> Error b
    
    let Ok = Choice1Of2
    let Error = Choice2Of2
    
    let bind f result = 
        match result with
        | Ok a -> f a
        | Error b -> Error b
    
    let map f result = 
        match result with
        | Ok a -> Ok(f a)
        | Error b -> Error b
    
    let get result = 
        match result with
        | Ok a -> a
        | Error b -> failwith (sprintf "%A" b)
