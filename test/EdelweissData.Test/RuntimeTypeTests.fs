module EdelweissData.Tests.RuntimeTypeTests

open Expecto
open System.Linq
open EdelweissData.Provided.Types
open EdelweissData.Provider.Types

[<Tests>]
let tests =
    testList "Dataset" [
        test "can fetch correct data" {
            let config = { EdelweissUrl = "https://api.edelweissdata.com/"; Token = "" }
            let instance = Instance(config)
            let dataset = instance.GetDataset("af50155c-8442-438c-96da-d57834cd4cbc")

            let output =
                dataset.Rows
                |> Seq.map(fun r -> r.["Description"] :?> string)
                |> Seq.head

            Expect.equal output "AR binding affinity was determined using competitive AR binding assay with [3H]R1884" "Retrieved data did not match expected"
        }
    ]