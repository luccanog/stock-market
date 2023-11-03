# Stock Market API

GraphQL API to Buy, Sell and track acquired Stocks performance.

## How to run

1. Go to the project folder
``` cmd
cd stock-market
```

2. Run a Docker compose up
``` cmd
docker-compose up
```
You may see the **graphql-api** running along with **event-processor** and **history-fetcher**

3. Go to `localhost/graphql` end send request through the UI


> NOTE: No need to run Kafka or DB locally, both services are hosted on Azure.

## About

* **Stock.Market.WebApi.GraphQL** - The main application where the graphql commands are going to be handle.

* **Stock.Market.HistoryFetcher** - Background service in charge of retrieving Nasdaq data and update the StockHistory.

* **Stock.Market.EventProcessor** - Background service which consumes Kafka events to Buy or Sell shares.

* **Stock.Market.Data** - Library with Entities, DBContext and Migrations.

* **Stock.Market.Common** - Library with shared code.

<img src='https://github.com/luccanog/stock-market/assets/55986783/37668f97-9031-49e8-ae87-0285524829c5' width='768'>


## GraphQL commands

#### Buy a stock

``` graphql
mutation{
  buyStockShares(quantity: 5, symbol: "AAPL")
}
```

#### Sell a stock

``` graphql
mutation{
  sellStockShares(quantity: 2, symbol: "AAPL")
}
```


#### Get a list of the stocks you are holding

``` graphql
query{
  stockData{
    symbol,
    variation,
    sharesHeld,
    totalValue,
    currentDayReferencePrices{
      lowestPrice,
      averagePrice,
      highestPrice
    }
  }
}
```

#### Get the Historical price of a stock you bought

``` graphql
query{
  priceHistory(symbol: "AAPL"){
    companyName,
    symbol,
    quotes{
      date,
      price
    }
  }
}
```
