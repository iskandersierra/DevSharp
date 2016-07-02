namespace DevSharp.Testing


open System
open DevSharp
open DevSharp.Domain.Aggregates.AggregateBehavior


type UnionDef =
    { 
        unionName: string; 
        cases: UnionCaseDef list; 
    }
and UnionCaseDef =
    {
        caseName: string;
        types: Type list;
    }


[<AbstractClass>]
type TestingMock<'Call>() =
    let mutable _calls : 'Call list = []
    member this.add c = do _calls <- _calls @ [ c ]
    member this.calls = _calls
    member this.clear () = _calls <- []
