# FSharp-OrderMatchingEngine
Simulation of Stock exchange order-book matching mechanism

This is a simulation of stock order matching engine in F#. 
The first *buy market order* get rejected as there are no sell orders in the orderbook.
![Image](images/first-order.png)

The nest *sell limit order* gets queued in the orderbook
![Image](images/second-order.png)

The next *buy market order* gets queued in the orderbook as there are no matching sell orders.
![Image](images/third-order.png)

The above order gets filled when its price is *amanded* to match the top sell order in the orderbook
![Image](images/fourth-amend-order.png)

Orders can be removed from orderbook
![Image](images/sixth-cancel-order.png)
