namespace RealWorld.Effects

module Actions = 
  open Suave
  open RealWorld.Models
  open DB
  open MongoDB.Bson
  open System.Text
  let jsonToString (json: 'a) = json |> Suave.Json.toJson |> System.Text.Encoding.UTF8.GetString

  let fakeReply email = 
    {user = { email = email; token = ""; username=""; bio=""; image=""; PasswordHash=""; }; Id=(BsonObjectId(ObjectId.GenerateNewId()))  }
    
  let registerUserNewUser dbClient = 
    request ( fun inputGraph -> 
      Suave.Json.fromJson<UserRequest> inputGraph.rawForm
      |> registerWithBson dbClient 
      |> RealWorld.Convert.userRequestToUser 
      |> jsonToString 
      |> Successful.OK
    )
      
  let getCurrentUser dbClient =
    request (fun inputGraph ->
      Successful.OK (fakeReply "" |> jsonToString)
    )

  let updateUser dbClient = 
    request (fun inputGraph ->
      let userToUpdate = (Suave.Json.fromJson<User> inputGraph.rawForm).user

      userToUpdate
      |> updateRequestedUser dbClient
      |> RealWorld.Convert.updateUser userToUpdate
      |> Successful.OK
    )

  open RealWorld.Stubs
  let getUserProfile dbClient username = 
    (Successful.OK Responses.singleProfile)

  let createNewArticle (articleToAdd : Article) dbCLient = 
    // TODO: add success response
    let succesful = insertNewArticle articleToAdd dbCLient
    articleToAdd 

  let getArticlesBy slug dbClient =
    (* TODO: Add suave testing for this. *)
    getArticleBySlug dbClient slug
    |> RealWorld.Convert.extractArticleList
    |> jsonToString
    |> Successful.OK

  let defaultTagsIfEmpty = function
    | Some tags -> tags
    | None -> { tags = [||] }

  let defaultArticleIfEmpty = function
    | Some articles -> Array.ofList articles
    | None -> [||]

  let getTagList dbClient =
    getSavedTagList dbClient 
    |> defaultTagsIfEmpty
    |> jsonToString
    |> Successful.OK 

  (* TODO: Look into consolidating these functions since they are close in functionality *)
  let getArticles dbClient = 
    getSavedArticles dbClient
    |> defaultArticleIfEmpty
    |> jsonToString
    |> Successful.OK

  let getArticlesForFeed dbClient = 
    getSavedFollowedArticles dbClient
    |> defaultArticleIfEmpty
    |> jsonToString
    |> Successful.OK

  let addArticleWithSlug json (slug: string) (dbClient: MongoDB.Driver.IMongoDatabase) = 
    let currentArticle = json |> Suave.Json.fromJson<Article> 
    let updatedSlug = { currentArticle.article with slug = slug}
    
    insertNewArticle ({currentArticle with article = updatedSlug }) dbClient
    |> jsonToString
    |> Successful.OK
    