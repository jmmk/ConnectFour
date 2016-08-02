// Adapted from https://gist.github.com/fdecampredon/10638794
// With help from ts2fable

namespace Fable.Import
open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Import.JS

module mori =
    type [<AllowNullLiteral>] [<Import("Vector","mori")>] Vector<'T>() =
        member __.____guardVector with get(): obj = failwith "JS only" and set(v: obj): unit = failwith "JS only"

    type [<Import("*","mori")>] Vector =
        static member vector([<ParamArray>] contents: 'T[]): Vector<'T> = failwith "JS only"
        static member conj(vector: Vector<'T>, value: 'T): Vector<'T> = failwith "JS only"
        static member assoc(vector: Vector<'T>, index: int, value: 'T): Vector<'T> = failwith "JS only"
        static member nth(vector: Vector<'T>, index: int, ?notFound: 'T): 'T = failwith "JS only"
        static member count(vector: Vector<'T>): int = failwith "JS only"
        static member map(callback: Func<'T, 'U>, vector: Vector<'T>): Vector<'U> = failwith "JS only"