# FSharp-OrderMatchingEngine
Simulation of Stock exchange order-book matching mechanism in F#
 

The first **_buy market order_** get rejected as there are no sell orders in the orderbook.
![Image](images/first-order.png)

then the first **_sell limit order_** gets queued in the orderbook (as its a **_limit_** order and there are no Buy orders to fill)
![Image](images/second-order.png)

Next, **_buy limit order_** gets queued in the orderbook as there are no matching sell orders.
![Image](images/third-order.png)

The above order gets filled when its price is **_amanded_** to match the top sell order in the orderbook
![Image](images/fourth-amend-order.png)

Orders can be removed from orderbook
![Image](images/sixth-cancel-order.png)
