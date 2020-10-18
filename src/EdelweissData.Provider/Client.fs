module EdelweissData.Provider.Client

open System
open System.Collections.Generic
open System.Data
open Newtonsoft.Json
open System.Threading.Tasks
open FSharp.Data
open EdelweissData.Provider.Types


let toNetType =
    function
    | "xsd:anyURI" -> typeof<Uri>
    | "xsd:boolean" -> typeof<bool>
    | "xsd:integer" -> typeof<int>
    | "xsd:double" -> typeof<double>
    | "xsd:dateTime" -> typeof<DateTime>
    | "xsd:date" -> typeof<DateTime>
    | _ -> typeof<string>


let convert (dataType: Type) (value: string) =
    match dataType with
    | t when t = typeof<Uri> -> Uri(value) :> obj
    | t when t = typeof<bool> -> Convert.ToBoolean(value) |> box
    | t when t = typeof<int> -> Convert.ToInt32(value) |> box
    | t when t = typeof<Double> -> Double.Parse(value) |> box
    | t when t = typeof<DateTime> -> DateTime.Parse(value) |> box
    | _ -> value :> obj

let toDataTable (schema: (string * Type)[]) (csv: CsvFile) =
    let table = new DataTable()

    let dataColumns = csv.Headers |> Option.map(fun h -> h.Length) |> Option.defaultValue 0
    if schema.Length <> dataColumns then
        // failwith (sprintf "%A" data)
        let message = sprintf "Dataset Data and Schema mismatch. Data contains %d columns while schema has %d columns" dataColumns schema.Length
        failwith message

    let columns = schema |> Array.map (fun (name, atype) -> new DataColumn(name,atype))

    //Create the Columns
    table.Columns.AddRange(columns)

    //Add the Data
    for csvRow in csv.Rows do
        let row = table.NewRow()
        for column in columns do
            try
                let value = csvRow.[column.ColumnName] |> convert column.DataType
                row.[column.ColumnName] <- value
            with
            | e -> ()
        table.Rows.Add(row) |> ignore
    table

let private getDataExpensive (config: Config) (dataset: DatasetInfo) =
    async {
        let url = sprintf "%s/datasets/%O/versions/%d/data" config.EdelweissUrl dataset.Id.Id dataset.Id.Version

        let headers = [
            "Accept", "text/csv"
            "Authorization", sprintf "Bearer %s" config.Token
        ]

        let! content = Http.AsyncRequestString(url, headers = headers, httpMethod = "POST")
        let csvData = FSharp.Data.CsvFile.Parse(content)
        let schema = dataset.Schema.Columns |> Array.map (fun c -> c.Name, c.DataType |> toNetType)
        let data = csvData |> toDataTable schema
        return data
    }
    |> Async.RunSynchronously

let private getDatasetsExpensive (config: Config) =
    async {

        let url = sprintf "%s/datasets" config.EdelweissUrl

        let headers = [
            "Accept", "text/csv"
            "Authorization", sprintf "Bearer %s" config.Token
        ]

        let body =
            """
            {
                "includeSchema": true
            }
            """

        let! content = Http.AsyncRequestString(url, headers = headers, httpMethod = "POST", body = TextRequest body)

        let response = JsonConvert.DeserializeObject<DatasetResponse>(content)
        return response.Results
    }
    |> Async.RunSynchronously

let getData =
    let cache = Dictionary<_,_>()
    fun (config: Config) (dataset: DatasetInfo) ->
        let key = sprintf "%s+%O" config.EdelweissUrl dataset.Id.Id
        let exist, value = cache.TryGetValue (key)
        match exist with
        | true -> 
            value
        | _ -> 
            // Function call is required first followed by caching the result for next call with the same parameters
            let value = getDataExpensive config dataset
            cache.Add (key, value)
            value
let getDatasets =
    let cache = Dictionary<_,_>()
    fun (config: Config) ->
        let key = sprintf "%s" config.EdelweissUrl
        let exist, value = cache.TryGetValue (key)
        match exist with
        | true -> 
            value
        | _ -> 
            // Function call is required first followed by caching the result for next call with the same parameters
            let value = getDatasetsExpensive config
            cache.Add (key, value)
            value