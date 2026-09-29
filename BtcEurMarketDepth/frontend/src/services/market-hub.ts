import {
  HubConnectionBuilder,
  LogLevel,
  type HubConnection,
} from '@microsoft/signalr'
import type { OrderBookSnapshot } from '../types/order-book-snapshot'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export function createMarketHub(
  onOrderBookUpdated: (snapshot: OrderBookSnapshot) => void,
): HubConnection {
  const connection = new HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}/hubs/market`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build()

  connection.on('OrderBookUpdated', onOrderBookUpdated)

  return connection
}
