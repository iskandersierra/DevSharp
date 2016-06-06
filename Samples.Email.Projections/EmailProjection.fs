[<DevSharp.Annotations.InstanceProjectionModule>]
module Samples.EmailProjection

open System
open DevSharp.Messaging
open Samples.Email.Logic

type Instance =
    {
        id:        Guid;
        subject:   EmailSubject;
        from:      FromEmail;
        toEmails:  ToEmails;
        ccEmails:  CcEmails;
        bccEmails: BccEmails;
        body:      EmailBody;
    }

let selectId (event: Event) (request: Request) : string option =
    request.instanceId

let update (instance: Instance) (event: Event) (request: Request) : Instance option =
    match event with 
    | FromWasUpdated (id, from) -> Some { instance with from = from; }
    | ToWasUpdated (id, toEmails) -> Some { instance with toEmails = toEmails; }
    | CcWasUpdated (id, ccEmails) -> Some { instance with ccEmails = ccEmails; }
    | BccWasUpdated (id, bccEmails) -> Some { instance with bccEmails = bccEmails; }
    | SubjectWasUpdated (id, subject) -> Some { instance with subject = subject; }
    | BodyWasUpdated (id, body) -> Some { instance with body = body; }
    // | WasDeleted -> None
    | _ -> Some instance