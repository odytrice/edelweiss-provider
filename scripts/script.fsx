#I @"C:\Projects\Github\edelweiss-provider\src\EdelweissData.Provider\bin\Debug\netstandard2.0"

#r "FSharp.Data.dll"
#r "FSharp.Data.DesignTime.dll"
#r "Newtonsoft.Json.dll"
#r "EdelweissData.Provider.dll"

open System.Data
open EdelweissData.Provided.Types
open EdelweissData.Provider.Types

//Test Type Provider
type PublicInstance = EdelweissData<Token = "">

let instance = PublicInstance()

instance.``Test file``.Rows