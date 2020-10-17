namespace EdelweissData.Provided.Types

open EdelweissData.Provider.Types
open System.Data
open System
open EdelweissData.Provider

type Dataset(config: Config, dataset: DatasetInfo) =
    member _.Rows =
        dataset
        |> Client.getData config
        |> fun d -> d.Rows
        |> Seq.cast<DataRow>

type Instance(config: Config) =
    member _.Datasets =
        config |> Client.getDatasets

    member this.GetDataset(datasetId: Guid) =
        let dataset =
            this.Datasets
            |> Array.filter(fun d -> d.Id.Id = datasetId)
            |> Array.tryHead

        match dataset with
        | Some dataset -> dataset
        | None -> failwith (sprintf "Dataset %O was not found" datasetId)