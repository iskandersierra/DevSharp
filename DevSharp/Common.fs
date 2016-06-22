[<AutoOpen>]
module DevSharp.Common

open System
open FSharp.Core

// Request definitions

let TenantIdConstant         = "TNT_ID"
let ApplicationIdConstant    = "APP_ID"
let ProjectIdConstant        = "PRJ_ID"
let UserIdConstant           = "USR_ID"
let SessionIdConstant        = "SES_ID"
let AggregateIdConstant      = "AGG_ID"
let CommandIdConstant        = "CMD_ID"
let AggregateTypeConstant    = "AGG_TP"
let CommandTypeConstant      = "CMD_TP"
let AggregateVersionConstant = "AGG_VR"
let ClientDateConstant       = "CLI_DT"
let ApiDateConstant          = "API_DT"
let ProcessDateConstant      = "PRC_DT"

type PropertiesType   = Map<string, obj>
type TenantId         = string
type ApplicationId    = string
type ProjectId        = string
type UserId           = string
type SessionId        = string
type AggregateId      = string
type CommandId        = string
type AggregateType    = string
type AggregateVersion = int
type RequestDate      = DateTimeOffset

type CommandType      = obj
type EventType        = obj
type StateType        = obj

type Request =
    {
        properties: PropertiesType
        tenantId:   TenantId
        userId:     UserId
        sessionId:  SessionId
        clientDate: RequestDate
        apiDate:    RequestDate
    }

type AggregateRequest =
    {
        request:       Request
        aggregateId:   AggregateId
        aggregateType: AggregateType
    }

type CommandRequest =
    {
        aggregate:        AggregateRequest
        aggregateVersion: AggregateVersion
        commandId:        CommandId
        commandType:      string
        processDate:      RequestDate
    }

let getRequestProperty onFound onNotFound (key: string) (properties: PropertiesType) =
    match properties |> Map.tryFind key with
    | None -> onNotFound ()
    | Some o ->
        match o with
        | :? 'a as result -> onFound result
        | _ -> onNotFound ()
    
let getRequestPropertyValue key properties =
    getRequestProperty
        (fun a -> Some a)
        (fun () -> None)
        key
        properties

let toRequest (properties: PropertiesType) : Request option =
    let tenantId:   TenantId option    = properties |> getRequestPropertyValue TenantIdConstant
    let userId:     UserId option      = properties |> getRequestPropertyValue UserIdConstant
    let sessionId:  UserId option      = properties |> getRequestPropertyValue SessionIdConstant
    let clientDate: RequestDate option = properties |> getRequestPropertyValue ClientDateConstant
    let apiDate:    RequestDate option = properties |> getRequestPropertyValue ApiDateConstant
    match tenantId, userId, sessionId, clientDate, apiDate with
    | Some tid, Some uid, Some sid, Some cdt, Some adt ->
        Some {
            properties = properties
            tenantId   = tid
            userId     = uid
            sessionId  = sid
            clientDate = cdt
            apiDate    = adt
        }
    | _ -> None

let toAggregateRequest (properties: PropertiesType) : AggregateRequest option =
    let request = properties |> toRequest
    let aggregateId:   AggregateId option   = properties |> getRequestPropertyValue AggregateIdConstant
    let aggregateType: AggregateType option = properties |> getRequestPropertyValue AggregateTypeConstant
    match request, aggregateId, aggregateType with
    | Some r, Some aid, Some atp ->
        Some {
            request = r
            aggregateId = aid
            aggregateType = atp
        }
    | _ -> None

let toCommandRequest (properties: PropertiesType) : CommandRequest option =
    let aggReq = properties |> toAggregateRequest
    let aggVer:      AggregateVersion option = properties |> getRequestPropertyValue AggregateVersionConstant
    let commandId:   CommandId option        = properties |> getRequestPropertyValue CommandIdConstant
    let commandType: string option           = properties |> getRequestPropertyValue CommandTypeConstant
    let processDate: RequestDate option      = properties |> getRequestPropertyValue ProcessDateConstant
    match aggReq, aggVer, commandId, commandType, processDate with
    | Some ar, Some aver, Some cid, Some ctp, Some pdt ->
        Some {
            aggregate = ar
            aggregateVersion = aver
            commandId = cid
            commandType = ctp
            processDate = pdt
        }
    | _ -> 
        None

// Observable definitions

type OnNextFunc<'a> = 'a -> unit
type OnCompletedFunc<'b> = unit -> 'b
type OnErrorFunc<'b> = exn -> 'b

type CompletionFuncs<'b> = 
    {
        onCompleted: OnCompletedFunc<'b>
        onError: OnErrorFunc<'b>
    }
type ObserverFuncs<'a, 'b> = 
    {
        onNext: OnNextFunc<'a>
        onCompleted: OnCompletedFunc<'b>
        onError: OnErrorFunc<'b>
    }

let completion onCompleted onError : CompletionFuncs<'b> =
    {
        onCompleted = onCompleted
        onError = onError
    }

let observer onNext onCompleted onError =
    {
        onNext = onNext
        onCompleted = onCompleted
        onError = onError
    }
