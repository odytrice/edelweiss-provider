module EdelweissData.Tests.ProviderTests

open Expecto
open EdelweissData.Provided.Types

type PublicInstance = EdelweissData<Token = "">

[<Tests>]
let tests =
    testList "Edelweiss Provider" [
        test "can resolve properties" {
            let instance = PublicInstance()

            let rows = instance.``Test file``.Rows

            let output = sprintf "%O" rows
            Expect.equal "" output ""
        }
    ]