namespace DevFSharp.Domain

open System
open System.Reflection
open System.Linq
open System.Linq.Expressions
open FSharp.Core
open FSharp.Collections
open FSharp.Reflection
open DevFSharp.Annotations
open DevFSharp.Validations
open DevFSharp.ReflectionUtils

type CommandType = obj
type EventType   = obj
type StateType   = obj

type IAggregateClass =

    abstract member validateCommand: CommandType -> ValidationResult

    abstract member processCommand: StateType -> CommandType -> EventType seq

    abstract member receiveEvent: StateType -> EventType -> StateType

type ModuleAggregateClass (aggregateModule: Type) =
    
    let aggregateName = aggregateModule.Name
    
    let aggregateNamespace = aggregateModule.Namespace

    let aggregateFullName = aggregateModule.FullName

    let containerName = sprintf "aggregate %O" aggregateFullName

    let commandType = 
        findModuleInnerType
            aggregateModule
            containerName
            typedefof<AggregateCommandsAttribute> 
            "Command"
    
    let eventType = 
        findModuleInnerType
            aggregateModule
            containerName
            typedefof<AggregateEventsAttribute> 
            "Event"
    
    let stateType =
        findModuleInnerType
            aggregateModule
            containerName
            typedefof<AggregateStateAttribute> 
            "State"

    let stateOptionType = 
        getOptionType stateType

    let validateInvoker =
        let validateCommandMethod = 
            findModuleMethod
                aggregateModule
                containerName
                typedefof<ValidateAggregateCommandAttribute> 
                "validate"
                ( getSeqType typedefof<ValidationItem> )
                [ commandType ]

        // command => MyModule.validate ((MyModule.Command)command).Cast<object>()
        let commandParameter = Expression.Parameter(typedefof<obj>, "command");
        let castExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, commandParameter, commandType)
        let callValidate = Expression.Call(validateCommandMethod, castExpr)
        //let callCast = Expression.Call(getMethodEnumerableCast typedefof<obj>, callValidate)
        let lambdaExpr = Expression.Lambda<Func<obj, ValidationItem seq>>(callValidate, commandParameter)
        let compiled = lambdaExpr.Compile()
        compiled

    let processCommandInvoker =
        let processCommandMethod = 
            findModuleMethod
                aggregateModule
                containerName
                typedefof<ProcessAggregateCommandAttribute> 
                "act"
                ( getListType eventType )
                [ getOptionType stateType; commandType ]

        // (astate, command) => MyModule.act ((MyModule.State option)astate, (MyModule.Command)command).Cast<object>()
        let astateParameter = Expression.Parameter(typedefof<obj>, "astate");
        let commandParameter = Expression.Parameter(typedefof<obj>, "command");
        let castAStateExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, astateParameter, stateOptionType)
        let castCommandExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, commandParameter, commandType)
        let callAct = Expression.Call(processCommandMethod, castAStateExpr, castCommandExpr)
        let callCast = Expression.Call(getMethodEnumerableCast typedefof<obj>, callAct)
        let lambdaExpr = Expression.Lambda<Func<obj, obj, obj seq>>(callCast, astateParameter, commandParameter)
        let compiled = lambdaExpr.Compile()
        compiled

    let receiveEventInvoker =
        let receiveEventMethod = 
            findModuleMethod
                aggregateModule
                containerName
                typedefof<ReceiveAggregateEventAttribute> 
                "apply"
                ( getOptionType stateType )
                [ getOptionType stateType; eventType ]

        // (astate, event) => MyModule.apply ((MyModule.State option)astate, (MyModule.Event)event)
        let astateParameter = Expression.Parameter(typedefof<obj>, "astate");
        let eventParameter = Expression.Parameter(typedefof<obj>, "event");
        let castAStateExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, astateParameter, stateOptionType)
        let castEventExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, eventParameter, eventType)
        let callApply = Expression.Call(receiveEventMethod, castAStateExpr, castEventExpr)
        let lambdaExpr = Expression.Lambda<Func<obj, obj, obj>>(callApply, astateParameter, eventParameter)
        let compiled = lambdaExpr.Compile()
        compiled

    interface IAggregateClass with
        member this.validateCommand command =
            let items = validateInvoker.Invoke command
            validationResult items

        member this.processCommand astate command =
            processCommandInvoker.Invoke(astate, command)

        member this.receiveEvent astate event =
            receiveEventInvoker.Invoke(astate, event)
    
