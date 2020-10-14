module EdelweissData.Provider.Datasets

// open System
// open System.IO
// open System.Data

// let internal loadBlock (csvPath: string) =
//     let lines =  File.ReadAllLines(csvPath) |> Array.map(fun l -> l.Split(','))

//     let width = lines |> Array.map(fun l -> l.Length) |> Array.max
//     let height = lines.Length

//     let block = Array2D.create height width ""
//     for i in 0 .. lines.Length - 1 do
//         for j in 0 .. lines.[i].Length - 1 do
//             block.[i, j] <- lines.[i].[j]

//     block


// let internal detectType (column: string[]) =
//     let isDateTime items = items |> Array.forall(fun (i: string) -> DateTime.TryParse(i) |> fst)
//     let isfloat items = items |> Array.forall(fun (i: string) -> Single.TryParse(i) |> fst)
//     let isInt items = items |> Array.forall(fun (i:string) -> Int32.TryParse(i) |> fst)

//     if isDateTime column then
//         typeof<DateTime>
//     elif isfloat column then
//         typeof<float>
//     elif isInt column then
//         typeof<int>
//     else
//         typeof<string>

// let internal convert (dataType: Type) (value: string) =
//     match dataType with
//     | t when t = typeof<DateTime> -> DateTime.Parse(value) |> box
//     | t when t = typeof<Single> -> Single.Parse(value) |> box
//     | t when t = typeof<Int32> -> Int32.Parse(value) |> box
//     | _ -> value :> obj

// let internal toDataTable (schema: (string * Type)[]) (data: string [,]) =
//     let table = new DataTable()
//     let header = data.[0, *]

//     let columns = schema |> Array.map (fun (name, atype) -> new DataColumn ())

//     //Create the Columns
//     table.Columns.AddRange(columns)

//     //Add the Data
//     for i in 1 .. data.GetLength(1) - 1 do
//         let row = table.NewRow()
//         for j in 0 .. columns.Length - 1 do
//             row.[j] <- data.[i, j] |> convert columns.[j].DataType
//         table.Rows.Add(row) |> ignore
//     table

// let internal loadData(csvPath: string) =
//     csvPath
//     |> loadBlock
//     |> toDataTable

// // Put any runtime constructs here
// type CsvFile(csvPath: string, schema: (string * Type)[]) =
//     let dataTable = csvPath |> loadData
//     member _.Rows = dataTable.Rows |> Seq.cast<DataRow>
//     static member Load csvPath = loadData csvPath