module EdelweissData.Tests.ProviderTests

open Expecto
open EdelweissData.Provider.Types

 open EdelweissData.TypeProviders
 type Public = EdelweissDataProvider<"">

[<Tests>]
let tests =
    testList "Edelweiss Provider" [
        test "can resolve properties" {

            let instance = Public()
            let rows = instance.``Test file``.Rows

            let output = sprintf "%O" rows
            Expect.equal "" output ""
        }
    ]