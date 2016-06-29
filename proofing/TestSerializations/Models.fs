namespace Samples.Models

module SimpleModels =

    type Union =
    | UnionCase1
    | UnionCase2 of string
    | UnionCase3 of int * string

    type Record =
        {
            id: string;
            count: int;
        }

    let record i = {id = sprintf "Record #%d" i; count = i}

    type Tuple = string * int

    let tuple i = (sprintf "Tuple #%d" i, i)


module AvlTree =

    type Node<'a> =
    | Leaf of 'a
    | Branch of 'a * int * int * Node<'a> * Node<'a>

    let rec label t =
        match t with
        | Leaf a -> a
        | Branch (a, l, r, h, c) -> a

    let rec count t =
        match t with
        | Leaf _ -> 1
        | Branch (a, h, c, l, r) -> c

    let rec height t =
        match t with
        | Leaf _ -> 0
        | Branch (a, h, c, l, r) -> h

    let leaf a = Leaf a
    let branch l a r = 
        let h = 1 + max (height l) (height r)
        let c = (count l) + (count r) + 1
        Branch (a, h, c, l, r)

    let rec toSeq t =
        match t with
        | Leaf a -> seq { yield a }
        | Branch (a, h, c, l, r) -> 
            seq {
                yield! toSeq l
                yield a
                yield! toSeq r
            }

    let toList t = toSeq t |> Seq.toList

    let toArray t = toSeq t |> Seq.toArray

    let sampleTree (labels: seq<'a>) maxHeight branchProbability lFactor rFactor =
        let random = System.Random()
        let enum = labels.GetEnumerator()
        let mutable hasNext = true
        let mutable currentLabel = Unchecked.defaultof<'a>
        let nextLabel () = 
            if hasNext then
                hasNext <- enum.MoveNext()
                if hasNext then 
                    currentLabel <- enum.Current
            currentLabel

        let rec genTree height bp =
            if height <= 0 
            then leaf (nextLabel()) 
            else 
                let left = genTreeOrLeaf (height - 1) bp lFactor
                let label = nextLabel()
                let right = genTreeOrLeaf (height - 1) bp rFactor
                branch left label right
        and genTreeOrLeaf height bp factor =
            match random.NextDouble() >= bp with
            | true -> leaf (nextLabel()) 
            | false -> genTree (height - 1) (bp * factor)


        genTree maxHeight branchProbability

    let sampleTree1 = sampleTree (seq { 1..100 } |> Seq.map (fun i -> i.ToString())) 1 1.0 1.0 1.0
    let sampleTree2 = sampleTree (seq { 1..100 } |> Seq.map (fun i -> i.ToString())) 6 1.0 1.0 1.0

