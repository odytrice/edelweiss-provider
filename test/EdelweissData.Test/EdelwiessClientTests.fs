module EdelweissData.Tests.EdelweissClientTests

open Expecto
open EdelweissData.Provider.EdelweissClient

let edelweissData = "https://api.edelweissdata.com/"
let token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6Ik0wTTVORFl5UXprd01qQXlSVFF3T0RORFFrVXlNVGswTWpCRVJUZEZOelUzUTBFd01UWXdPUSJ9.eyJodHRwczovL2NsYWltcy5lZGVsd2Vpc3MuZG91Z2xhc2Nvbm5lY3QuY29tL2VtYWlsIjoib2R5dHJpY2VAZ21haWwuY29tIiwiaXNzIjoiaHR0cHM6Ly9lZGVsd2Vpc3MuZXUuYXV0aDAuY29tLyIsInN1YiI6Imdvb2dsZS1vYXV0aDJ8MTA1MTExNDMyODM3MDkyNDc1NDk2IiwiYXVkIjpbImh0dHBzOi8vYXBpLmVkZWx3ZWlzc2RhdGEuY29tIiwiaHR0cHM6Ly9lZGVsd2Vpc3MuZXUuYXV0aDAuY29tL3VzZXJpbmZvIl0sImlhdCI6MTYwMjU4ODc0MCwiZXhwIjoxNjAyNjc1MTQwLCJhenAiOiJ3ZnNacnlsUWdkNzlabVdkUTdMWjlBT2FsdHhqUWJoZyIsInNjb3BlIjoib3BlbmlkIGVtYWlsIG9mZmxpbmVfYWNjZXNzIn0.QV_l4PS7mQxYQXfGWSMZgKLggHdIdPq0ECaXrfqm9kGMfZwp_P40zJwAUmbiMUj2UfEp9etwv3WIMJKfdh4gJwAHryT9A8DgZvDSwQBmPTMpOxigCq8IDPbxikYibu2h5pio-A95JJBc0PPBdm_gnP71RFQgDs6HV6XLa8qLvJ-XfV8_lRl_Q5mLAd6thgYV-luu16YOHafJ3ohtNLPi-XWWyruiecAxNEEY8KW5uezPNXAhRs3Q3_6SUjUq201VbKzb9SDeOw7woa1yTQ7SU3j2DUiT87Jc7_3Lqje9IYEWC6dqLIdfzU8t3y_tvRBdWM9pbLKjm_wyx4INGeyYFw"

[<Tests>]
let tests =
    testList "Edelweiss Client" [
        testAsync "can fetch datasets" {
            let! datasets = getDatasets edelweissData token
            let hasDatasets = datasets.Length > 0
            Expect.isTrue hasDatasets "No Datasets were returned"
        }
    ]