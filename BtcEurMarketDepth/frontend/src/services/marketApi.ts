import type { BuyQuote } from '../types/BuyQuote'
import type { OrderBookSnapshot } from '../types/OrderBookSnapshot'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5272'

export async function getOrderBook(): Promise<OrderBookSnapshot> {
  const response = await fetch(`${apiBaseUrl}/api/market/order-book`)

  if (!response.ok) {
    throw new Error(`Failed to load order book. Status: ${response.status}`)
  }

  return response.json() as Promise<OrderBookSnapshot>
}

export async function getBuyQuote(quantity: number): Promise<BuyQuote> {
  const response = await fetch(`${apiBaseUrl}/api/market/quote?quantity=${quantity}`)

  if (!response.ok) {
    throw new Error(`Failed to load buy quote. Status: ${response.status}`)
  }

  return response.json() as Promise<BuyQuote>
}