﻿namespace Canaan

[<AutoOpen>]
module Api =
     
    type ApiResult<'TSuccess,'TFailure> = 
        | Success of 'TSuccess
        | Failure of 'TFailure
        with member x.Res with get() = match x with | Success s -> s | Failure f -> failwith "This result is a failure."

     //API Logging
    let inline info mt args = Api.Info(mt, List.toArray args)

    let inline debug mt args = Api.Debug(mt, List.toArray args)

    let inline err mt args = Api.Error(mt, List.toArray args)

    let inline errex mt args = Api.Error(mt, List.toArray args)

    //API Patterns
    let (|Default|) defaultValue input =    
        defaultArg input defaultValue
    
    //API Logic
    let inline tryCatch f x =
        try
            f x |> Success
        with
        | ex -> 
            err "jj" []
            Failure ex

    let inline tryCatch' f x =
        try
            f x 
        with
        | ex -> 
            err "jj" []
            Failure ex

    let inline tryCatchAsync' f x = tryCatch' Async.RunSynchronously << f <| x
    
    let bind f = 
        function
        | Success value -> tryCatch f value
        | Failure failure -> Failure failure

    let bind' f = 
        function
        | Success value -> tryCatch' f value
        | Failure failure -> Failure failure
    
    let bind'' f = 
        function
        | Success value -> f value |> Success
        | Failure failure -> Failure failure


    let test = 
        function
        | Success _ -> true
        | Failure _ -> false

    let succ = 
        function
        | Success value -> value
        | Failure failure -> failwith "This API result is failure."

    let init (api: 'T when 'T :> Api) = if api.Initialized then api else failwith "This API object is not initialized."

    let init' (api: 'T when 'T :> Api) = if api.Initialized then Success api else Failure (exn "This API object is not initialized.")

    let switch' res f = 
        match res with
        | Success _ -> tryCatch' f
        | Failure failure -> fun _ -> Failure failure
    
    let switchAsync' res f =
        match res with
        | Success _ -> tryCatchAsync' f
        | Failure failure -> fun _ -> Failure failure

    let try' f x = tryCatch' f x

    let try'' f x = tryCatchAsync' f x

    let inline (?) (l:bool) (r, j) = if l then r else j

    let inline (!?) (x:Option<_>) = x.IsSome

    let inline (!>) f = tryCatch f

    let inline (!>>) f = tryCatch' f   
    
    let inline (!>>>) f = tryCatchAsync' f

    let inline (!!) (api: #Api) = init api

    let inline (!!>) (api: #Api) = init' api 

    let inline (!>?) res = test res

    let inline (|>>) res f = switch' res f

    let inline (|>>>) res f = switchAsync' res f

    let inline (>>|) api f = api |> init' |> switch' <| f
    
    let inline (>>>|) api f = api |> init' |> switchAsync' <| f

    let inline (>>=) f1 f2  = bind f2 f1 

    let inline (>>>=) f1 f2  = bind' f2 f1
    
    let inline (>>@=) f1 f2  = bind'' f2 f1

    let inline (<<@=) f1 f2  = bind'' f1 f2

    let inline (>=>) f1 f2 = tryCatch' f1 >> (bind' f2)