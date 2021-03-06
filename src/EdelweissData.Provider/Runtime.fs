namespace EdelweissData.Provider.Types

open System.Data
open System
open EdelweissData.Provider

type Dataset(config: Config, dataset: DatasetInfo) =
    let table = lazy(dataset |> Client.getData config)

    member _.Rows =
        table.Value
        |> fun d -> d.Rows
        |> Seq.cast<DataRow>

    member _.Columns =
        table.Value
        |> fun d -> d.Columns
        |> Seq.cast<DataColumn>

type Instance(config: Config) =
    
    let datasets = lazy(config |> Client.getDatasets)
    
    member _.Datasets = datasets.Value        

    member this.GetDataset(datasetId: string) =
        let datasetOption =
            datasets.Value
            |> Array.filter(fun d -> d.Id.Id.ToString() = datasetId)
            |> Array.tryHead

        match datasetOption with
        | Some datasetInfo -> Dataset(config, datasetInfo)
        | None -> failwith (sprintf "Dataset %O was not found" datasetId)