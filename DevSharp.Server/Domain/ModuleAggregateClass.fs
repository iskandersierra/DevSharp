namespace DevSharp.Server.Domain

open System
open System.Linq.Expressions
open FSharp.Core
open FSharp.Collections
open DevSharp.Messaging
open DevSharp.Annotations
open DevSharp.Validations
open DevSharp.Validations.ValidationUtils
open DevSharp.Domain.Aggregates
open DevSharp.Server
open DevSharp.Server.ReflectionUtils

type validateDelegate = Func<obj, Request, ValidationItem seq>
type actDelegate = Func<obj, obj, Request, obj seq option>
type applyDelegate = Func<obj, obj, Request, obj>

type ModuleAggregateClass (aggregateModule: Type) =
    
    //let aggregateName = aggregateModule.Name
    
    //let aggregateNamespace = aggregateModule.Namespace

    let aggregateFullName = aggregateModule.FullName

    let containerName = sprintf "aggregate %O" aggregateFullName

    let requestType = typedefof<Request>

    let (initValue, isStatelessValue, stateType) =
        let property =
            findModuleProperty
                aggregateModule
                containerName
                typedefof<AggregateInitAttribute> 
                "init"
                true

        if property = null 
        then (null, true, typeOfObj)
        else (property.GetValue(null), false, property.PropertyType)

    let (actInvoker, actRequiresRequest, commandType, eventType) =
        let actMethod = 
            findModuleMethod
                aggregateModule
                containerName
                typedefof<AggregateActAttribute> 
                "act"
                false

        let parameters = actMethod.GetParameters()
        let paramCount = parameters.Length
        let cmdType = parameters.[0].ParameterType
        let evType =
            match matchGenericClass typeOfOption actMethod.ReturnType with
            | Some [ listType ] ->
                match matchGenericClass typeOfList listType with
                | Some [ eventType ] ->
                    eventType
                | _ -> failwith (sprintf "function %s of module %s should return an Event list option" actMethod.Name aggregateFullName)
            | _ -> failwith (sprintf "function %s of module %s should return an Event list option" actMethod.Name aggregateFullName)


        let commandParameter = Expression.Parameter(typedefof<obj>, "command");
        let stateParameter = Expression.Parameter(typedefof<obj>, "state");
        let requestParameter = Expression.Parameter(requestType, "request")

        let castCommandExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, commandParameter, cmdType)
        let castStateExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, stateParameter, stateType)

        // MyModule.act (command, ?state, ?request)
        let (callAct, requiresRequest) = 
            match paramCount with
            | 1 -> (Expression.Call(actMethod, castCommandExpr), false)
            | 2 -> (Expression.Call(actMethod, castCommandExpr, castStateExpr), false)
            | 3 -> (Expression.Call(actMethod, castCommandExpr, castStateExpr, requestParameter), true)
            | _ -> raise (NotSupportedException (sprintf "Aggregate function %s cannot have %i parameters" actMethod.Name paramCount))

        // MyModule.act (...).fromListOptToSeqOpt()
        let finalExpr = 
            Expression.Call(methodFromListOptToSeqOpt.MakeGenericMethod(evType), callAct)

        // (command, state, request) => MyModule.act (...)....
        let lambdaExpr = Expression.Lambda<actDelegate>(finalExpr, commandParameter, stateParameter, requestParameter)

        let compiled = lambdaExpr.Compile()
        (compiled.Invoke, requiresRequest, cmdType, evType)


    let (validateInvoker, validateRequiresRequest) =
        let actMethod = 
            findModuleMethod
                aggregateModule
                containerName
                typedefof<AggregateValidateAttribute> 
                "validate"
                true

        match actMethod with
        | null ->
            ((fun _ -> Seq.empty), false)
        | _ ->

            let paramCount = actMethod.GetParameters().Length

            // (command, request) => MyModule.validate ((MyModule.Command)command)
            let commandParameter = Expression.Parameter(typedefof<obj>, "command")
            let requestParameter = Expression.Parameter(requestType, "request")
            let castCommandExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, commandParameter, commandType)
            let (callValidate, requiresRequest) = 
                match paramCount with
                | 1 -> (Expression.Call(actMethod, castCommandExpr), false)
                | 2 -> (Expression.Call(actMethod, castCommandExpr, requestParameter), true)
                | _ -> raise (NotSupportedException (sprintf "Aggregate function %s cannot have %i parameters" actMethod.Name paramCount))

            let lambdaExpr = Expression.Lambda<validateDelegate>(callValidate, commandParameter, requestParameter)
            let compiled = lambdaExpr.Compile()
            (compiled.Invoke, requiresRequest)


    let (applyInvoker, applyRequireRequest) =
        let actMethod = 
            findModuleMethod
                aggregateModule
                containerName
                typedefof<AggregateApplyAttribute> 
                "apply"
                true

        match actMethod with
        | null ->
            ((fun (_, state, _) -> state), false)
        | _ ->
            let paramCount = actMethod.GetParameters().Length

            let eventParameter = Expression.Parameter(typedefof<obj>, "event");
            let stateParameter = Expression.Parameter(typedefof<obj>, "state");
            let requestParameter = Expression.Parameter(requestType, "request")

            let castEventExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, eventParameter, eventType)
            let (callApply, requiresRequest) = 
                if paramCount = 1 then
                    // (event, state, request) => MyModule.apply ((MyModule.Event)event)
                    (Expression.Call(actMethod, castEventExpr), false)
                else 
                    let castStateExpr = Expression.MakeUnary(ExpressionType.ConvertChecked, stateParameter, stateType)
                    if paramCount = 2 then
                        // (event, state, request) => MyModule.apply ((MyModule.Event), event(MyModule.State)state)
                        (Expression.Call(actMethod, castEventExpr, castStateExpr), false)
                    else
                        if paramCount = 3 then
                            // (event, state, request) => MyModule.apply ((MyModule.Event), event(MyModule.State)state, request)
                            (Expression.Call(actMethod, castEventExpr, castStateExpr, requestParameter), true)
                        else
                            raise (NotSupportedException (sprintf "Aggregate function %s cannot have %i parameters" actMethod.Name paramCount))

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

    override this.ToString() = sprintf "Aggregate class %s" aggregateFullName

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
