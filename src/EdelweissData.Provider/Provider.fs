namespace EdelweissData.Provider

open FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System.Reflection
open EdelweissData.Provider.Types
open System.Data
open Microsoft.FSharp.Quotations


[<TypeProvider>]
type EdelweissProvider(config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config)

    // Get the assembly and namespace used to house the provided types.
    let asm = Assembly.GetExecutingAssembly()
    let ns = "EdelweissData.Provided.Types"

    // Create the main provided type.
    let provider = ProvidedTypeDefinition(asm, ns, "EdelweissData", Some(typeof<DataRow>))

    let createRowType dataset =
        let rowType = ProvidedTypeDefinition("Row", Some(typeof<DataRow>))
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
        let datasetId = datasetInfo.Id.Id.ToString()
        let datasetType = ProvidedTypeDefinition(datasetId, Some typeof<Dataset>)

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

    let addDatasets token edelweissUrl (instanceType: ProvidedTypeDefinition) =
        let config = { Token = token; EdelweissUrl = edelweissUrl }
        let datasetInfos = Client.getDatasets config

        for datasetInfo in datasetInfos do
            let datasetId = datasetInfo.Id.Id.ToString()

            let datasetType = createDatasetType datasetInfo

            let datasetGetter (args: Expr list) =
                <@@
                    let instance = %%args.[0]:Instance
                    instance.GetDataset(datasetId)
                @@>
            let prop = ProvidedProperty(datasetInfo.Name, datasetType, isStatic = false, getterCode = datasetGetter)
            instanceType.AddMember prop
            instanceType.AddMember datasetType


    let createType typeName (args: obj[]) =
        let instanceType = ProvidedTypeDefinition(asm, ns, typeName, Some(typeof<Instance>))

        let edelweissUrl = args.[0] :?> string
        let token = args.[1] :?> string

        let ctorCode (_: Expr list) =
            <@@
                let config = { Token = token; EdelweissUrl = edelweissUrl }
                Instance(config)
            @@>
        let ctor = ProvidedConstructor([], invokeCode = ctorCode)
        instanceType.AddMember(ctor)

        instanceType |> addDatasets token edelweissUrl

        instanceType


    //Declare Static Type Parameters
    let parameters = [
        ProvidedStaticParameter("Url", typeof<string>, "https://api.edelweissdata.com")
        ProvidedStaticParameter("Token", typeof<string>)
    ]

    // Add the type to the namespace.
    do
        provider.DefineStaticParameters(parameters, createType)
        this.AddNamespace(ns, [ provider ])


[<assembly: TypeProviderAssembly>]
do ()