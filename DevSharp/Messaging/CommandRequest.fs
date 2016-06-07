namespace DevSharp.Messaging

open System
open DevSharp


type CommandRequest (properties: Map<string, obj>) =

    let getPropertyTask onFound onNotFound key =
        match Map.tryFind key properties with
        | None -> onNotFound ()
        | Some o ->
            match o with
            | :? 'a as result -> onFound result
            | _ -> onNotFound ()
    
    let getProperty key =
        getPropertyTask
            (fun a -> Some a)
            (fun () -> None)
            key

    member this.properties = properties

    //member this.get = getPropertyTask

    member this.getTenantId onFound onNotFound = getPropertyTask onFound onNotFound TenantIdConstant
    member this.tenantId : TenantId option = getProperty TenantIdConstant

    member this.getApplicationId onFound onNotFound = getPropertyTask onFound onNotFound ApplicationIdConstant
    member this.applicationId : ApplicationId option = getProperty ApplicationIdConstant

    member this.getProjectId onFound onNotFound = getPropertyTask onFound onNotFound ProjectIdConstant
    member this.projectId : ProjectId option = getProperty ProjectIdConstant

    member this.getUserId onFound onNotFound = getPropertyTask onFound onNotFound UserIdConstant
    member this.userId : UserId option = getProperty UserIdConstant

    member this.getSessionId onFound onNotFound = getPropertyTask onFound onNotFound SessionIdConstant
    member this.sessionId : SessionId option = getProperty SessionIdConstant

    member this.getAggregateId onFound onNotFound = getPropertyTask onFound onNotFound AggregateIdConstant
    member this.aggregateId : AggregateId option = getProperty AggregateIdConstant

    member this.getAggregateVersion onFound onNotFound = getPropertyTask onFound onNotFound AggregateVersionConstant
    member this.aggregateVersion : AggregateVersion option = getProperty AggregateVersionConstant


