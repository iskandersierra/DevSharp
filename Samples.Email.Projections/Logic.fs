[<DevSharp.Annotations.AggregateModule>]
module Samples.Email.Logic

open System
open DevSharp.Validations
open DevSharp.Validations.ValidationUtils
open DevSharp.Annotations

type EmailSubject           = string
type FromEmail              = string
type ToEmails               = string list
type CcEmails               = string list
type BccEmails              = string list
type EmailId                = Guid
type EmailBody              = string

type Event = 
| WasCreated                
| FromWasUpdated            of EmailId * FromEmail
| ToWasUpdated              of EmailId * ToEmails
| CcWasUpdated              of EmailId * CcEmails
| BccWasUpdated             of EmailId * BccEmails
| SubjectWasUpdated         of EmailId * EmailSubject
| BodyWasUpdated            of EmailId * EmailBody
| EmailWasRemoved           of EmailId
| TaskWasSent               of EmailId
