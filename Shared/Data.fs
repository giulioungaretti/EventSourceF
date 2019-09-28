module Data

open System
open Newtonsoft.Json

open Microsoft.Azure.Cosmos.Table

let inline notNull value = if (obj.ReferenceEquals(value, null))  then None else Some value

type EventC(payload:string, timestamp:DateTimeOffset, partitionKey, rowKey) =
    inherit TableEntity(partitionKey, rowKey)


    new() = EventC(null, DateTimeOffset.UtcNow, null, null)
    new(payload:string, timestamp:DateTimeOffset, partitionKey) = EventC(payload, timestamp,  partitionKey, timestamp.Ticks.ToString())

    member val ID = Guid.NewGuid() with get, set
    member val TimeStamp = timestamp with get, set
    member val Payload = payload with get,set

    member this.GeTvalue<'T> (): Result<'T, string> =
        try
            JsonConvert.DeserializeObject<'T> this.Payload
            |> Ok
        with
            | :? JsonException as exn ->
                Error exn.Message


let Store (table:CloudTable) (event: EventC)=
    let tableOperation = TableOperation.Insert(event)
    table.ExecuteAsync(tableOperation)

type AggregatorSate<'s> =
    | Valid of 's
    | Empty
    | Invalid of string

[<CLIMutable>]
type EventAggregatorState =
    {
    State: string option
    LastEventAggregatedTimestamp: DateTimeOffset option
    LastEventAggregatedID: System.Guid option
    }

    static member Serialize record =
         JsonConvert.SerializeObject record

    member this.Deserialize(): AggregatorSate<'T> =
        try
            match this.State with
            | Some x ->
                JsonConvert.DeserializeObject<'T> x
                |> Valid
            | None ->
                Empty
        with
            | :? JsonException as exn ->
                Invalid exn.Message


//[Serializable]
//  public class EventAggregatorState
//  {
//      public string PayloadType { get; set; }
//      public string PayloadJson { get; set; }
//      public Guid LastEvent { get; set; }
//      public DateTimeOffset? LastEventTimestamp { get; set; } = null;

//      public List<(BusinessEvent, Exception)> Failures { get; set; } = new List<(BusinessEvent, Exception)>();

//      public void SetValue<T>(T payload)
//      {
//          PayloadType = typeof(T).AssemblyQualifiedName;
//          PayloadJson = JsonConvert.SerializeObject(payload);
//      }

//      public T GetValue<T>()
//      {
//          try
//          {
//              var payloadType = Type.GetType(PayloadType);
//              return (T)JsonConvert.DeserializeObject(PayloadJson, payloadType);
//          }
//          catch
//          {
//              return default;
//          }
//      }
//  }
