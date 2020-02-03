# FSharp-OrderMatchingEngine
Simulation of Stock exchange order-book matching mechanism

This is a simulation of stock order matching engine in F#. 

The first **_buy market order_** get rejected as there are no sell orders in the orderbook.
![Image](images/first-order.png)

The nest **_sell limit order_** gets queued in the orderbook
![Image](images/second-order.png)

The next **_buy market order_** gets queued in the orderbook as there are no matching sell orders.
![Image](images/third-order.png)

The above order gets filled when its price is **_amanded_** to match the top sell order in the orderbook
![Image](images/fourth-amend-order.png)

Orders can be removed from orderbook
![Image](images/sixth-cancel-order.png)
