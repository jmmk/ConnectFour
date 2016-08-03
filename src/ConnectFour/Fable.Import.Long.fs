// Adapted from https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/long/long.d.ts
// With help from ts2fable

namespace Fable.Import
open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Import.JS

module long =
    type [<AllowNullLiteral>] [<Import("*","long")>] Long(low: int, ?high: int, ?unsigned: bool) =
        static member ONE: Long = failwith "JS only"
        static member ZERO: Long = failwith "JS only"
        static member fromNumber(value: int64, ?unsigned: bool): Long = failwith "JS only"
        member __.``and``(other: Long): Long = failwith "JS only"
        member __.``and``(other: int): Long = failwith "JS only"
        member __.isZero(): bool = failwith "JS only"
        member __.``or``(other: Long): Long = failwith "JS only"
        member __.``or``(other: int): Long = failwith "JS only"
        member __.shiftLeft(numBits: Long): Long = failwith "JS only"
        member __.shiftLeft(numBits: int): Long = failwith "JS only"
        member __.shiftRight(numBits: Long): Long = failwith "JS only"
        member __.shiftRight(numBits: int): Long = failwith "JS only"
        member __.equals(other: Long): bool = failwith "JS only"


