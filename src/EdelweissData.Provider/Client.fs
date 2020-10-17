module EdelweissData.Provider.Client

open System
open System.Net.Http
open System.Net
open System.Data
open Newtonsoft.Json
open System.Threading.Tasks
open Types


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


let loadBlock (csvString: string) =
    let lines =  csvString.Split([| '\r'; '\n' |]) |> Array.map(fun l -> l.Split(','))

    let width = lines |> Array.map(fun l -> l.Length) |> Array.max
    let height = lines.Length

    let block = Array2D.create height width ""
    for i in 0 .. lines.Length - 1 do
        for j in 0 .. lines.[i].Length - 1 do
            block.[i, j] <- lines.[i].[j]

    block


let convert (dataType: Type) (value: string) =
    match dataType with
    | t when t = typeof<DateTime> -> DateTime.Parse(value) |> box
    | t when t = typeof<Single> -> Single.Parse(value) |> box
    | t when t = typeof<Int32> -> Int32.Parse(value) |> box
    | _ -> value :> obj

let toDataTable (schema: (string * Type)[]) (data: string [,]) =
    let table = new DataTable()
    let columns = schema |> Array.map (fun (name, atype) -> new DataColumn ())

    //Create the Columns
    table.Columns.AddRange(columns)

    //Add the Data
    for i in 1 .. data.GetLength(1) - 1 do
        let row = table.NewRow()
        for j in 0 .. columns.Length - 1 do
            row.[j] <- data.[i, j] |> convert columns.[j].DataType
        table.Rows.Add(row) |> ignore
    table

let loadData (schema: (string * Type) []) (csvString: string) =
    csvString
    |> loadBlock
    |> toDataTable schema



let getData (config: Config) (dataset: DatasetInfo) =
    async {
        let url = Uri(Uri(config.EdelweissUrl), sprintf "/datasets/%O/versions/%d/data" dataset.Id.Id dataset.Id.Version)
        let httpRequest = new HttpRequestMessage(HttpMethod.Post,url)
        httpRequest.Headers.Add("Accept", "text/csv")
        httpRequest.Headers.Add("Authorization", sprintf "Bearer %s" config.Token)
        let! response = client.SendAsync(httpRequest) |> Async.AwaitTask
        if response.IsSuccessStatusCode then
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let schema = dataset.Schema.Columns |> Array.map (fun c -> c.Name, c.DataType |> toNetType)
            let data = loadData schema content
            return data
        else
            return failwith (sprintf "Could not fetch data from Edelweiss: %O" response.ReasonPhrase)
    }
    |> Async.RunSynchronously

let getDatasets (config: Config) =
    async {
        let url = Uri(Uri(config.EdelweissUrl),  "/datasets")

        let httpRequest = new HttpRequestMessage(HttpMethod.Post,url)
        httpRequest.Headers.Add("Accept", "application/json")
        httpRequest.Headers.Add("Authorization", sprintf "Bearer %s" config.Token)

        let payload =
            """
            {
                "includeSchema": true
            }
            """

        let content = new StringContent(payload)
        httpRequest.Content <- content

        let! response = client.SendAsync(httpRequest) |> Async.AwaitTask

        if response.IsSuccessStatusCode then
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let response = JsonConvert.DeserializeObject<DatasetResponse>(content)
            return response.Results
        else
            return failwith (sprintf "Could not fetch data from Edelweiss: %O" response.ReasonPhrase)
    }
    |> Async.RunSynchronously
