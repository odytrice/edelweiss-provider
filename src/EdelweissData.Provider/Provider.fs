namespace EdelweissData.Provider

open FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System.Reflection
open EdelweissData.Provider.Types
open System.Data
open Microsoft.FSharp.Quotations
open System


type Dataset(config: Config, dataset: DatasetInfo) =
    member this.Rows =
        dataset
        |> Client.getData config
        |> fun d -> d.Rows
        |> Seq.cast<DataRow>

type Instance(config: Config) =
    member this.Datasets =
        config |> Client.getDatasets

    member this.GetDataset(datasetId: Guid) =
        let dataset =
            this.Datasets
            |> Array.filter(fun d -> d.Id.Id = datasetId)
            |> Array.tryHead

        match dataset with
        | Some dataset -> dataset
        | None -> failwith (sprintf "Dataset %O was not found" datasetId)



[<TypeProvider>]
type EdelweissProvider(config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config)

    // Get the assembly and namespace used to house the provided types.
    let asm = Assembly.GetExecutingAssembly()
    let ns = "EdelweissData.TypeProviders"

    // Create the main provided type.
    let provider = ProvidedTypeDefinition(asm, ns, "EdelweissDataProvider", Some(typeof<DataRow>))


    let createRowType dataset =
        let rowType = ProvidedTypeDefinition("Row", Some(typeof<obj>))
        for i in 0 .. dataset.Schema.Columns.Length - 1 do
            let column = dataset.Schema.Columns.[i]

            let rowGetter (args: Expr list) =
                <@@
                    let row = %%args.[0]: DataRow
                    unbox row.[i]
                @@>

            let prop =
                ProvidedProperty(column.Name,
                    column.DataType |> Client.toNetType,
                    getterCode = rowGetter)

            rowType.AddMember(prop)
        rowType

    let createDatasetType (datasetInfo: DatasetInfo) =
        let datasetType = ProvidedTypeDefinition(datasetInfo.Id.Id.ToString(), Some typeof<Dataset>)

        let rowType = createRowType datasetInfo

        let rowsGetter (args: Expr list) =
            <@@
                let dataset = %%args.[0]: Dataset
                dataset.Rows
            @@>

        // Add a more strongly typed Data property, which uses the existing property at runtime.
        let prop =
            ProvidedProperty("Rows",
                typedefof<seq<_>>.MakeGenericType(rowType),
                getterCode = rowsGetter)

        datasetType.AddMember prop
        datasetType.AddMember rowType

        datasetType


    let createType typeName (args: obj[]) =
        // Define the provided type, erasing to CsvFile.
        let instanceType = ProvidedTypeDefinition(asm, ns, typeName, Some(typeof<Instance>))

        let token = args.[0] :?> string
        let edelweissUrl = args.[1] :?> string


        let ctorCode (_: Expr list) =
            <@@
                let config = { Token = token; EdelweissUrl = edelweissUrl }
                Instance(config)
            @@>
        let ctor = ProvidedConstructor([], invokeCode = ctorCode)
        instanceType.AddMember ctor


        let config = { Token = token; EdelweissUrl = edelweissUrl }
        let datasetInfos = Client.getDatasets config
        for i in 0 .. datasetInfos.Length - 1 do
            let datasetInfo = datasetInfos.[i]
            let datasetId = datasetInfo.Id.Id

            let datasetType = createDatasetType datasetInfo

            let datasetGetter (args: Expr list) =
                <@@
                    let instance = (%%args.[0]:Instance)
                    instance.GetDataset(datasetId)
                @@>
            let prop = ProvidedProperty(datasetInfo.Name, datasetType, isStatic = true, getterCode = datasetGetter)
            instanceType.AddMember prop
            instanceType.AddMember datasetType

        instanceType


    //Declare Static Type Parameters
    let parameters = [
        ProvidedStaticParameter("Token", typeof<string>, "")
        ProvidedStaticParameter("Url", typeof<string>, "https://api.edelweissdata.com/")
    ]

    // Add the type to the namespace.
    do
        provider.DefineStaticParameters(parameters, createType)
        this.AddNamespace(ns, [ provider ])


[<assembly: TypeProviderAssembly>]
do ()