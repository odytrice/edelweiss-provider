namespace EdelweissData.Provider

open FSharp.Core.CompilerServices

open ProviderImplementation
open ProviderImplementation.ProvidedTypes
open System.Reflection
open EdelweissClient



[<TypeProvider>]
type EdelweissProvider(config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config)

    // Get the assembly and namespace used to house the provided types.
    let asm = Assembly.GetExecutingAssembly()
    let ns = "EdelweissData.TypeProviders"

    // Create the main provided type.
    let provider = ProvidedTypeDefinition(asm, ns, "EdelweissDataProvider", Some(typeof<obj>))

    //Declare Static Type Parameters
    let parameters = [
        ProvidedStaticParameter("Token", typeof<string>)
        ProvidedStaticParameter("Url", typeof<string>, "https://api.edelweissdata.com/")
    ]

    let generateType (dataset: Dataset) =
        let datasetId = dataset.Id.Id.ToString()
        let datasetType = ProvidedTypeDefinition(datasetId, baseType = Some typeof<obj>)

        for column in dataset.Schema.Columns do
            let prop = ProvidedProperty(column.Name, column.DataType |> toNetType)
            datasetType.AddMember(prop)

        datasetType


    let addDatasets (datasets: Dataset array) (typeName: ProvidedTypeDefinition) =
        for dataset in datasets do
            let a = ""
            let prop = ProvidedProperty(dataset.Name, typeof<obj>)
            typeName.AddMember prop

    let createType typeName (args: obj[]) =
        // Define the provided type, erasing to CsvFile.
        let typeName = ProvidedTypeDefinition(asm, ns, typeName, Some(typeof<obj>))

        //Fetch Datasets
        let token = args.[0] :?> string
        let edelweissUrl = args.[1] :?> string

        let datasets = EdelweissClient.getDatasets edelweissUrl token |> Async.RunSynchronously

        typeName |> addDatasets datasets

        typeName

    // Add the type to the namespace.
    do
        provider.DefineStaticParameters(parameters, createType)
        this.AddNamespace(ns, [ provider ])

    do
        System.AppDomain.CurrentDomain.add_AssemblyResolve(fun _ args ->
            let name = System.Reflection.AssemblyName(args.Name)
            let existingAssembly =
                System.AppDomain.CurrentDomain.GetAssemblies()
                |> Seq.tryFind(fun a -> System.Reflection.AssemblyName.ReferenceMatchesDefinition(name, a.GetName()))
            match existingAssembly with
            | Some a -> a
            | None -> null
            )


[<assembly: TypeProviderAssembly>]
do ()