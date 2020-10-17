module EdelweissData.Tests.ProviderTests

open Expecto
open System.Linq
open EdelweissData.Provided.Types

type PublicInstance = EdelweissData<Token = "">

[<Tests>]
let tests =
    testList "Edelweiss Provider" [
        test "can resolve properties" {
            let instance = PublicInstance()

            let output =
                instance.``Test file``.Rows
                |> Seq.map(fun r -> r.Description)
                |> Seq.head

            Expect.equal output "AR binding affinity was determined using competitive AR binding assay with [3H]R1884" "Retrieved data did not match expected"
        }
    ]