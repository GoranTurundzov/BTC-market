import type { OrderBookSnapshot } from '../types/market'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5272'

export async function getOrderBook(): Promise<OrderBookSnapshot> {
  const response = await fetch(`${apiBaseUrl}/api/market/order-book`)

  if (!response.ok) {
    throw new Error(`Failed to load order book. Status: ${response.status}`)
  }

  return response.json() as Promise<OrderBookSnapshot>
}