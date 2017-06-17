namespace Helper
  module Utils = 
    open MongoDB.Bson
    open MongoDB.Driver
    open MongoDB.Driver.Linq
    open Suave
    open Suave.Web

    let withUri url httpCtx =
        let uri = new System.Uri("http://some.phony.url" + url)
        let rawQuery = uri.Query.TrimStart('?')
        let req = { httpCtx.request with url = uri; rawQuery = rawQuery;  }
        { httpCtx with request = req }

    let asGetRequest hc =
        let req = { hc.request with ``method`` = HttpMethod.GET }
        { hc with request = req }

    let GetRequest u =
        HttpContext.empty
        |> withUri u
        |> asGetRequest

    let databaseClient =
      let mongoConn : string = "mongodb://localhost:27017"
      let client = new MongoClient(mongoConn)
      client.GetDatabase("realworld")

    let possibleResult = function
      | Some a -> a
      | None -> failwith "Expected result from router."

    let extractContext ctx =
      ctx |> Async.RunSynchronously |> possibleResult

    let getContent content = 
      match content with
      | Bytes a -> a
      | _ -> failwith "Didn't return string content."