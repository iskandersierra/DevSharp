namespace DevSharp.Testing


open System


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


