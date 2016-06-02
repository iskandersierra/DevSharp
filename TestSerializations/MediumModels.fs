module MediumModels

open System

type PostState =
| Draft        = 0
| Published    = 1
| Hidden       = 2

type DeletePost = DeletePost of DeletePostData
and DeletePostData =
    {
        postId: int
        referenceId: Guid option
        lastModified: DateTime
        deletedBy: uint64 option
        reason: string
        versions: uint64 array
        state: PostState option
        votes: bool option list
    }

let deletePost id = 
    let now = DateTime.Now
    {
        postId = id
        deletedBy = id / 100 |> uint64 |> Some
        lastModified = now
        referenceId = if id % 10 < 8 then Guid.NewGuid() |> Some else None
        reason = "for no reason"
        versions = [| 1 .. id % 10 |] |> Array.map uint64
        state = None
        votes = [ 1 .. (id % 10) ] |> List.map (fun i -> match i % 3 with | 0 -> Some true | 1 -> Some false | _ -> None)
    }

type Post = Post of PostData
and PostData =
    {
        id: int
        title: string
        text: string
        created: DateTime
        tags: Set<string>
        approved: DateTime option
        comments: Comment list
        votes: Vote
        notes: string list
        state: PostState
    }
and Comment = Comment of CommentData
and CommentData = 
    {
        created: DateTime
        approved: DateTime option
        user: string
        message: string
        votes: Vote
    }
and Vote = Vote of VoteData
and VoteData = 
    {
        upvote: int
        downvote: int
    }

let post id =
    let now = DateTime.Now
    {
        id = id 
        approved = if id % 10 < 8 then Some now else None
        created = now.AddMinutes(- float id)
        votes = Vote {upvote = id / 13; downvote = id / 25}
        text = sprintf "Some text describing post #%A" id
        title = sprintf "post #%A" id
        state = Enum.ToObject (typedefof<PostState>, 1) :?> PostState
        tags = [1..(id % 5)] |> List.map (fun i -> sprintf "tag%A" i) |> List.toSeq |> Set
        comments = [1..(id % 100)] 
        |> List.map (fun i -> 
            Comment {
                created = now.AddMinutes(float i)
                approved = if id % 7 < 3 then Some now else None
                user = sprintf "some user #%A" i
                message = sprintf "comment #%A for post #%A" i id
                votes = Vote {upvote = (id + i) / 17; downvote = (id + i) / 19}
            })
        notes = [1..(id % 7)] |> List.map (fun i -> sprintf "note #%A" i)
    }
