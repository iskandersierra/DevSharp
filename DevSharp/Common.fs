[<AutoOpen>]
module DevSharp.Common

let TenantIdConstant         = "TNT_ID"
let ApplicationIdConstant    = "APP_ID"
let ProjectIdConstant        = "PRJ_ID"
let UserIdConstant           = "USR_ID"
let SessionIdConstant        = "SES_ID"
let AggregateIdConstant      = "AGG_ID"
let AggregateVersionConstant = "AGG_VER"

type TenantId         = string
type ApplicationId    = string
type ProjectId        = string
type UserId           = string
type SessionId        = string
type AggregateId      = string
type AggregateVersion = int

type CommandType      = obj
type EventType        = obj
type StateType        = obj

