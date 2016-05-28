namespace DevSharp.Server.Domain

open System
open System.Linq.Expressions
open FSharp.Core
open FSharp.Collections
open DevSharp.Messaging
open DevSharp.Annotations
open DevSharp.Validations
open DevSharp.Validations.ValidationUtils
open DevSharp.Domain
open DevSharp.Server

type validateDelegate = Func<obj, Request, ValidationItem seq>
type actDelegate = Func<obj, obj, Request, obj seq>
type applyDelegate = Func<obj, obj, Request, obj>

type ModuleAggregateClass (aggregateModule: Type) =
    
    //let aggregateName = aggregateModule.Name
    
    //let aggregateNamespace = aggregateModule.Namespace

    let aggregateFullName = aggregateModule.FullName

    let containerName = sprintf "aggregate %O" aggregateFullName

    let commandType = 
        ReflectionUtils.findModuleInnerType
            aggregateModule
            containerName
            typedefof<AggregateCommandAttribute> 
            "Command"
            false
    
    let eventType = 
        ReflectionUtils.findModuleInnerType
            aggregateModule
            containerName
            typedefof<AggregateEventAttribute> 
            "Event"
            false


    let (initValue, isStatelessValue, stateType) =
        let property =
            ReflectionUtils.findModuleProperty
                aggregateModule
                containerName
                typedefof<AggregateInitAttribute> 
                "init"
                true

        if property = null 
        then (null, true, typedefof<obj>)
        else (property.GetValue(null), false, property.PropertyType)

    let (validateInvoker, validateRequiresRequest) =
        let method = 
            ReflectionUtils.findModuleMethod
                aggregateModule
                containerName
                typedefof<AggregateValidateAttribute> 
                "validate"
                ( ReflectionUtils.getSeqType typedefof<ValidationItem> )
                [ commandType; ]
                true

        match method with
        | null ->
            ((fun _ -> Seq.empty), false)
        | _ ->

            let paramCount = method.GetParameters().Length

            // (command, request) => MyModule.validate ((MyModule.Command)command)
            let commandParameter = Expression.Parameter(typedefof<obj>, "command")
            let requestParameter = Expression.Parameter(typedefof<Request>, "request")
            let castCommandExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, commandParameter, commandType)
            let (callValidate, requiresRequest) = 
                if paramCount = 1 then
                    (Expression.Call(method, castCommandExpr), false)
                else
                    if paramCount = 2 then
                        // (command, request) => MyModule.validate ((MyModule.Command)command, request)
                        (Expression.Call(method, castCommandExpr, requestParameter), true)
                    else
                        raise (NotSupportedException (sprintf "Aggregate function %s cannot have %i parameters" method.Name paramCount))
            let lambdaExpr = Expression.Lambda<validateDelegate>(callValidate, commandParameter, requestParameter)
            let compiled = lambdaExpr.Compile()
            (compiled.Invoke, requiresRequest)

    let (actInvoker, actRequiresRequest) =
        let method = 
            ReflectionUtils.findModuleMethod
                aggregateModule
                containerName
                typedefof<AggregateActAttribute> 
                "act"
                ( ReflectionUtils.getListType eventType )
                [ commandType; stateType; ]
                true

        let paramCount = method.GetParameters().Length

        let commandParameter = Expression.Parameter(typedefof<obj>, "command");
        let stateParameter = Expression.Parameter(typedefof<obj>, "state");
        let requestParameter = Expression.Parameter(typedefof<Request>, "request")

        let castCommandExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, commandParameter, commandType)
        let (callAct, requiresRequest) = 
            if paramCount = 1 then
                // (command, state, request) => MyModule.act ((MyModule.Command)command).Cast<object>()
                (Expression.Call(method, castCommandExpr), false)
            else 
                let castStateExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, stateParameter, stateType)
                if paramCount = 2 then
                    // (command, state, request) => MyModule.act ((MyModule.Command)command, (MyModule.State)state).Cast<object>()
                    (Expression.Call(method, castCommandExpr, castStateExpr), false)
                else
                    if paramCount = 3 then
                        // (command, state, request) => MyModule.act ((MyModule.Command)command, (MyModule.State)state, request).Cast<object>()
                        (Expression.Call(method, castCommandExpr, castStateExpr, requestParameter), false)
                    else
                        raise (NotSupportedException (sprintf "Aggregate function %s cannot have %i parameters" method.Name paramCount))

        let callCast = Expression.Call(ReflectionUtils.getMethodEnumerableCast typedefof<obj>, callAct)
        let lambdaExpr = Expression.Lambda<actDelegate>(callCast, commandParameter, stateParameter, requestParameter)
        let compiled = lambdaExpr.Compile()
        (compiled.Invoke, requiresRequest)


    let (applyInvoker, applyRequireRequest) =
        let method = 
            ReflectionUtils.findModuleMethod
                aggregateModule
                containerName
                typedefof<AggregateApplyAttribute> 
                "apply"
                ( stateType )
                [ eventType; stateType; ]
                true

        match method with
        | null ->
            ((fun (_, state, _) -> state), false)
        | _ ->
            let paramCount = method.GetParameters().Length

            let eventParameter = Expression.Parameter(typedefof<obj>, "event");
            let stateParameter = Expression.Parameter(typedefof<obj>, "state");
            let requestParameter = Expression.Parameter(typedefof<Request>, "request")

            let castEventExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, eventParameter, eventType)
            let (callApply, requiresRequest) = 
                if paramCount = 1 then
                    // (event, state, request) => MyModule.apply ((MyModule.Event)event)
                    (Expression.Call(method, castEventExpr), false)
                else 
                    let castStateExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, stateParameter, stateType)
                    if paramCount = 2 then
                        // (event, state, request) => MyModule.apply ((MyModule.Event), event(MyModule.State)state)
                        (Expression.Call(method, castEventExpr, castStateExpr), false)
                    else
                        if paramCount = 3 then
                            // (event, state, request) => MyModule.apply ((MyModule.Event), event(MyModule.State)state, request)
                            (Expression.Call(method, castEventExpr, castStateExpr, requestParameter), true)
                        else
                            raise (NotSupportedException (sprintf "Aggregate function %s cannot have %i parameters" method.Name paramCount))

            let castCall = Expression.MakeUnary(ExpressionType.Convert, callApply, typedefof<System.Object>);
            let lambdaExpr = Expression.Lambda<applyDelegate>(castCall, eventParameter, stateParameter, requestParameter)
            let compiled = lambdaExpr.Compile()
            (compiled.Invoke, requiresRequest)

    let requiresRequestValue = 
        validateRequiresRequest || actRequiresRequest || applyRequireRequest

    
    member this.init =
        initValue

    member this.isStateless = 
        isStatelessValue

    member this.requiresRequest = 
        requiresRequestValue

    member this.validate command request =
        let items = validateInvoker (command, request)
        validationResult items

    member this.act command state request =
        actInvoker(command, state, request)

    member this.apply event state request =
        applyInvoker(event, state, request)


    interface IAggregateClass with
        member this.init =
            this.init

        member this.isStateless = 
            this.isStateless

        member this.requiresRequest = 
            this.requiresRequest

        member this.validate command request =
            this.validate command request

        member this.act command state request =
            this.act command state request

        member this.apply event state request =
            this.apply event state request
