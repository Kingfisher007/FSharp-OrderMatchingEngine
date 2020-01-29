#load "OrderMatchingEngine.fs"

open System
open System.Collections.Generic
open OrderMatchingEngine.MatchingEngine


// setup order book for MSFT symbol
let bidComparer = BidComparer()
let offerComparer = OfferComparer()
let MSFTBook = { Symbol = "MSFT"; 
                Bids = new SortedSet<Order>(bidComparer); 
                Offers = new SortedSet<Order>(offerComparer); 
                BestBid = None; 
                BestOffer = None }


// this order should get rejected 
// as there are no SELL orders to fill this market order
let order1 =  { Id = "order1";
              Symbol = "MSFT";
              Kind = Buy;
              Quantity = 50;
              Price = None;
              timestamp = DateTime.Now }
//
processOrderBookRequest (NewOrder order1) MSFTBook
|> printOrderBookResult


// this order should get queued at OFFERS queue
let order2 =  { Id = "order2";
          Symbol = "MSFT";
          Kind = Sell;
          Quantity = 200;
          Price = Some(25.0);
          timestamp = DateTime.Now }
//
processOrderBookRequest (NewOrder order2) MSFTBook
|> printOrderBookResult

// this order should get queued at BIDS queue
// as there are no offers to SELL at 20.0 price
let order3 =  { Id = "order3";
          Symbol = "MSFT";
          Kind = Buy;
          Quantity = 50;
          Price = Some(20.0);
          timestamp = DateTime.Now }
//
processOrderBookRequest (NewOrder order3) MSFTBook
|> printOrderBookResult

// amend order3 with price 25
// this order should get executed
//
processOrderBookRequest (Amend (order3, Some(25.0), 50)) MSFTBook
|> printOrderBookResult

// this order should get executed
let order4 =  { Id = "order4";
          Symbol = "MSFT";
          Kind = Buy;
          Quantity = 50;
          Price = Some(30.0);
          timestamp = DateTime.Now }
//
processOrderBookRequest (NewOrder order4) MSFTBook
|> printOrderBookResult

// this order should get queued at OFFERS queue
let order5 =  { Id = "order5";
          Symbol = "MSFT";
          Kind = Sell;
          Quantity = 100;
          Price = Some(30.0);
          timestamp = DateTime.Now }
//
processOrderBookRequest (NewOrder order5) MSFTBook
|> printOrderBookResult

// this order should get executed
let order6 =  { Id = "order6";
          Symbol = "MSFT";
          Kind = Buy;
          Quantity = 250;
          Price = Some(30.0);
          timestamp = DateTime.Now }
//
processOrderBookRequest (NewOrder order6) MSFTBook
|> printOrderBookResult
    
// this order should get queued
let order7 =  { Id = "order7";
          Symbol = "MSFT";
          Kind = Sell;
          Quantity = 100;
          Price = Some(30.0);
          timestamp = DateTime.Now }
//
processOrderBookRequest (NewOrder order7) MSFTBook
|> printOrderBookResult

// this order should be cancelled
processOrderBookRequest (Cancel order7) MSFTBook
|> printOrderBookResult
    
    
