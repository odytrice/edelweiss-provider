module EdelweissData.Tests.EdelweissClientTests

open Expecto
open EdelweissData.Provider.Client
open EdelweissData.Provider.Types

let edelweissData = "https://api.edelweissdata.com"
let config = { EdelweissUrl = edelweissData; Token = "" }

[<Tests>]
let tests =
    testList "Edelweiss Client" [
        test "can fetch datasets" {
            let datasets = getDatasets config
            let hasDatasets = datasets.Length > 0
            Expect.isTrue hasDatasets "No Datasets were returned"
        }

        test "can fetch schema" {
            let dataset =
                getDatasets config
                |> Seq.filter(fun d -> d.Id.Id.ToString() = "af50155c-8442-438c-96da-d57834cd4cbc")
                |> Seq.head

            let hasColumns = dataset.Schema.Columns.Length > 0

            Expect.isTrue hasColumns "Dataset Schema was empty"
        }

        test "can fetch Dataset data" {
            let dataset =
                getDatasets config
                |> Seq.filter(fun d -> d.Id.Id.ToString() = "af50155c-8442-438c-96da-d57834cd4cbc")
                |> Seq.head

            let data = getData config dataset

            Expect.isTrue (data.Columns.Count > 0) "Datatable has no columns"
            Expect.isTrue (data.Rows.Count > 0) "Datatable has no rows"
        }
    ]