namespace OrderMatchingEngine

open System
open System.Collections.Generic


module MatchingEngine =
    
    type Kind =
        | Buy
        | Sell
    
    type Order = 
        { Id        : string
          Symbol    : string
          Kind      : Kind
          Quantity  : int
          Price     : double option
          timestamp : DateTime }

    // Type of request for new order
    type OrderBookRequest =
        | NewOrder of Order
        | Cancel of Order
        | Amend of Order * NewPrice : double option * NewQuantity : int

    // Result of procesing new order request
    type OrderBookResponse =
        | Filled of timestamp : DateTime * Price : double * Quantity : int * Orders : Order list
        | PartiallyFilled of timestamp : DateTime * Price : double * Quantity : int * Orders : Order list
        | Acknowledged of timestamp : DateTime * Request : OrderBookRequest
        | Rejected of timestamp : DateTime * Reason : String * Request : OrderBookRequest
        | Cancelled of timestamp : DateTime * Reason : string * Request : OrderBookRequest

    // Events for notifying the listeners for order changes
    type MarketEvent = 
        | LastSale of timestamp : DateTime * Symbol : string * Price : double * Quantity : single
        | BestBid of timestamp : DateTime * Symbol : string * BidPrice : double
        | BestOffer of timestamp : DateTime * Symbol : string * OfferPrice : double

    // Comparer to sort Bid orders
    type BidComparer() =
        interface IComparer<Order> with
            member this.Compare (leftOrder, rightOrder) =
                match (leftOrder.Price, rightOrder.Price) with
                | (Some(bp1), Some(bp2)) -> Convert.ToInt32(bp1 - bp2)
                | (None, Some(_)) -> -1
                | (Some(_), None) -> 1
                | (None, None) -> Convert.ToInt32(leftOrder.timestamp.Ticks - rightOrder.timestamp.Ticks)

    // Comparer to sort Sell orders
    type OfferComparer() =
        interface IComparer<Order> with
            member this.Compare (leftOrder, rightOrder) =
                match (leftOrder.Price, rightOrder.Price) with
                | (Some(bp1), Some(bp2)) -> Convert.ToInt32(bp2 - bp1)
                | (None, Some(_)) -> 1
                | (Some(_), None) -> -1
                | (None, None) -> Convert.ToInt32(rightOrder.timestamp.Ticks - leftOrder.timestamp.Ticks)

    // Order book for stock symbol
    type OrderBook = 
        { Symbol : string
          Bids : SortedSet<Order>
          Offers : SortedSet<Order> 
          BestBid : Order option
          BestOffer : Order option
        }

    // Validate the new order
    let validateOrder order =
        true
    
    // Check if new order can be processed and matched against existing orders
    let isLimitOrderExecutable order topOppositeOrder =
        match order.Kind with
        | Buy -> order.Price >= topOppositeOrder.Price
        | Sell -> order.Price <= topOppositeOrder.Price 

    // Get the order queue and opposite queue to match orders from
    let getQueues order orderBook =
        match order.Kind with
        | Buy -> (orderBook.Bids, orderBook.Offers)
        | Sell -> (orderBook.Offers, orderBook.Bids) 

    // Recursively match the order with existing orders from opposite queue
    let rec matchOrder order (queue:SortedSet<Order>) orderBook volumeSoFar (matchedOrders:Order list) =
        match queue.Count with
        | 0 -> PartiallyFilled(DateTime.Now, order.Price.Value, volumeSoFar, matchedOrders)
        | _ ->  let top = queue.Max
                if(order.Quantity < top.Quantity) then
                    let newQuantity = top.Quantity - order.Quantity
                    let modifiedOrder = { top with Quantity = newQuantity }
                    queue.Remove(top) |> ignore
                    queue.Add(modifiedOrder) |> ignore
                    Filled(DateTime.Now, top.Price.Value, order.Quantity, matchedOrders @ [{top with Quantity = order.Quantity }] ) 
                else if (order.Quantity > top.Quantity) then
                    let newQuantity = order.Quantity - top.Quantity
                    let newOrder = { order with Quantity = newQuantity }
                    queue.Remove(queue.Max) |> ignore
                    matchOrder newOrder queue orderBook (volumeSoFar + top.Quantity) (matchedOrders @ [top])
                else
                    queue.Remove(top) |> ignore
                    Filled(DateTime.Now, top.Price.Value, top.Quantity, matchedOrders @ [top])

    // Process the order of type New
    let processNewOrder (order:Order) orderBookRequest orderBook =
        let (oq, mq) = getQueues order orderBook 
        match order.Price with
        | None -> match mq.Count with
                  | 0 -> Rejected(DateTime.Now, "No orders to fill", orderBookRequest)
                  | n -> matchOrder order mq orderBook 0 []
        | Some(p) -> if (mq.Count = 0 || not (isLimitOrderExecutable order mq.Max)) then
                        oq.Add(order) |> ignore
                        Acknowledged(DateTime.Now, orderBookRequest)
                     else
                        matchOrder order mq orderBook 0 []


    // Cancel an order
    let cancelOrder (order:Order) orderBookRequest orderBook =
        let (oq,_) = getQueues order orderBook
        match oq.Remove(order) with
        | false -> Rejected(DateTime.Now, "order not found", orderBookRequest)
        | true -> Cancelled(DateTime.Now, "removed from order queue as per user request", orderBookRequest)

    // Amend an existing order and try to match amended order
    let amendOrder (order:Order) newPrice newQuantity orderBookRequest orderBook =
        let (oq,mq) = getQueues order orderBook
        match oq.Remove(order) with
        | false -> Rejected(DateTime.Now, "Order not found", orderBookRequest )
        | true -> let amendedOrder = { order with Price = newPrice; Quantity = newQuantity }
                  matchOrder amendedOrder mq orderBook 0 []


    // Process new order
    let processOrderBookRequest (obr:OrderBookRequest) (ob:OrderBook) =
        match obr with
        | NewOrder no -> processNewOrder no obr ob 
        | Cancel co ->  cancelOrder co obr ob
        | Amend (ao, np, nq) -> amendOrder ao np nq obr ob


    // Concat order ids
    let concat acc x =
        match String.IsNullOrEmpty(acc) with
        | true -> x.Id
        | false -> acc + "; " + x.Id
    
    // Print order processing response
    let printOrderBookResult obr =
        match obr with
        | Filled (time, price, quantity, orders) -> sprintf "Filled - Price - %f; Quantity - %i; Orders - {%s}" price quantity (orders |> List.fold concat "")
        | PartiallyFilled (time, price, quantity, orders) -> sprintf "Partially Filled - Price - %f; Quantity - %i; Orders - {%s}" price quantity (orders |> List.fold concat "")
        | Acknowledged (time, _)  -> sprintf "Acknowledged"
        | Cancelled (time, reason, _) -> sprintf "Cancelled - %s" reason
        | Rejected (time, reason, _) -> sprintf "Rejected - %s" reason
