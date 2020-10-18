namespace EdelweissData.Provider.Types

open System
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

type DatasetInfo = {
    Id: DatasetId
    Name: string
    Description: string
    Schema: Schema
}

type DatasetResponse = {
    Total: int
    Offset: int
    Results: DatasetInfo []
}

type Config =
    {
        EdelweissUrl: string
        Token: string
    }
