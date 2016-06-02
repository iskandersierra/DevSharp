module StartUp

type BenchTarget = 
| JsonNet        = 0
| MsgPack        = 1

type BenchSize =
| Small        = 0

type BenchType  =
| Serialization = 0
| Both          = 1
| None          = 2
| Check         = 3

