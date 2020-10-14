module EdelweissData.Provider.EdelweissClient

open System
open System.Net.Http
open System.Net
open Newtonsoft.Json
open System.Threading.Tasks



type DatasetId = {
    Id: Guid
    Version: int
}

type Column = {
    Name: string
    DataType: string
}

type Schema = {
    Columns: Column []
}

type Dataset = {
    Id: DatasetId
    Name: string
    Description: string
    Schema: Schema
}

type DatasetResponse = {
    Total: int
    Offset: int
    Results: Dataset []
}

let client = new HttpClient()

let toNetType =
    function
    | "xsd:anyURI" -> typeof<Uri>
    | "xsd:boolean" -> typeof<bool>
    | "xsd:integer" -> typeof<int>
    | "xsd:double" -> typeof<double>
    | "xsd:dateTime" -> typeof<DateTime>
    | "xsd:date" -> typeof<DateTime>
    | _ -> typeof<string>

let getDatasets (edelweissUrl: string) (token: string) =
    async {
        let url = Uri(Uri(edelweissUrl),  "/datasets")
        let httpRequest = new HttpRequestMessage(HttpMethod.Get,url)
        httpRequest.Headers.Add("Accept", "application/json")
        httpRequest.Headers.Add("Authorization", sprintf "Bearer %s" token)
        let! response = client.SendAsync(httpRequest) |> Async.AwaitTask
        if response.IsSuccessStatusCode then
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let response = JsonConvert.DeserializeObject<DatasetResponse>(content)
            return response.Results
        else
            return [||]
    }


let getData edelweissUrl token =




type Instance(edelweissUrl, token) =

    member this.Datasets() =
        getDatasets edelweissUrl token |> Async.RunSynchronously
    member this.GetData(datasetId: Guid) =
        getData edelweissUrl token
