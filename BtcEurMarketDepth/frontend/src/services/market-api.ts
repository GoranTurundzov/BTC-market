import type { BuyQuote } from '../types/buy-quote'
import type { OrderBookSnapshot } from '../types/order-book-snapshot'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export async function getOrderBook(): Promise<OrderBookSnapshot> {
  const response = await fetch(`${apiBaseUrl}/api/market/order-book`)

  if (!response.ok) {
    throw new Error(`Failed to load order book. Status: ${response.status}`)
  }

  return response.json() as Promise<OrderBookSnapshot>
}

export async function getOrderBookHistory(
  from: string,
  to: string,
): Promise<OrderBookSnapshot[]> {
  const query = new URLSearchParams({ from, to })
  const response = await fetch(
    `${apiBaseUrl}/api/market/order-book/history?${query.toString()}`,
  )

  if (!response.ok) {
    throw new Error(`Failed to load order-book history. Status: ${response.status}`)
  }

  return response.json() as Promise<OrderBookSnapshot[]>
}

export async function getBuyQuote(
  quantity: number,
  at?: string,
): Promise<BuyQuote> {
  const query = new URLSearchParams({ quantity: quantity.toString() })

  if (at) {
    query.set('at', at)
  }

  const response = await fetch(
    `${apiBaseUrl}/api/market/quote?${query.toString()}`,
  )

  if (!response.ok) {
    throw new Error(`Failed to load buy quote. Status: ${response.status}`)
  }

  return response.json() as Promise<BuyQuote>
}
