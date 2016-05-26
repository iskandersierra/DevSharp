namespace DevFSharp.Domain

open System
open System.Reflection
open System.Linq.Expressions
open FSharp.Core
open FSharp.Collections
open FSharp.Reflection
open DevFSharp.Annotations
open DevFSharp.Validations
open DevFSharp.ReflectionUtils

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


    let (initialStateValue, stateType) =
        let initialStateProperty =
            findModuleProperty
                aggregateModule
                containerName
                typedefof<AggregateInitialStateAttribute> 
                "initialState"
        (initialStateProperty.GetValue(null), initialStateProperty.PropertyType)

    let validateInvoker =
        let validateCommandMethod = 
            findModuleMethod
                aggregateModule
                containerName
                typedefof<ValidateAggregateCommandAttribute> 
                "validate"
                ( getSeqType typedefof<ValidationItem> )
                [ commandType; ]

        // command => MyModule.validate ((MyModule.Command)command).Cast<object>()
        let commandParameter = Expression.Parameter(typedefof<obj>, "command")
        let castExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, commandParameter, commandType)
        let callValidate = Expression.Call(validateCommandMethod, castExpr)
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
                [ commandType; stateType; ]

        // (state, command) => MyModule.act ((MyModule.State)state, (MyModule.Command)command).Cast<object>()
        let commandParameter = Expression.Parameter(typedefof<obj>, "command");
        let stateParameter = Expression.Parameter(typedefof<obj>, "state");
        let castCommandExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, commandParameter, commandType)
        let castStateExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, stateParameter, stateType)
        let callAct = Expression.Call(processCommandMethod, castCommandExpr, castStateExpr)
        let callCast = Expression.Call(getMethodEnumerableCast typedefof<obj>, callAct)
        let lambdaExpr = Expression.Lambda<Func<obj, obj, obj seq>>(callCast, commandParameter, stateParameter)
        let compiled = lambdaExpr.Compile()
        compiled

    let receiveEventInvoker =
        let receiveEventMethod = 
            findModuleMethod
                aggregateModule
                containerName
                typedefof<ReceiveAggregateEventAttribute> 
                "apply"
                ( stateType )
                [ eventType; stateType; ]

        // (state, event) => MyModule.apply ((MyModule.State)state, (MyModule.Event)event)
        let eventParameter = Expression.Parameter(typedefof<obj>, "event");
        let stateParameter = Expression.Parameter(typedefof<obj>, "state");
        let castEventExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, eventParameter, eventType)
        let castStateExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, stateParameter, stateType)
        let callApply = Expression.Call(receiveEventMethod, castEventExpr, castStateExpr)
        let lambdaExpr = Expression.Lambda<Func<obj, obj, obj>>(callApply, eventParameter, stateParameter)
        let compiled = lambdaExpr.Compile()
        compiled


    member this.initialState =
        initialStateValue

    member this.validateCommand command =
        let items = validateInvoker.Invoke command
        validationResult items

    member this.processCommand state command =
        processCommandInvoker.Invoke(state, command)

    member this.receiveEvent state event =
        receiveEventInvoker.Invoke(state, event)


    interface IAggregateClass with
        member this.initialState =
            this.initialState

        member this.validateCommand command =
            this.validateCommand command

        member this.processCommand command state =
            this.processCommand command state

        member this.receiveEvent event state =
            this.receiveEvent event state
