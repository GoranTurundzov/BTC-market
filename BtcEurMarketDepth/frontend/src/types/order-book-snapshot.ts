import type { PriceLevel } from "./price-level"

export interface OrderBookSnapshot {
  symbol: string
  acquiredAt: string
  bids: PriceLevel[]
  asks: PriceLevel[]
  sequence: number
}