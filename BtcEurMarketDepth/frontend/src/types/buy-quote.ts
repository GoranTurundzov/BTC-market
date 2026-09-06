export interface BuyQuote {
  requestedQuantity: number
  filledQuantity: number
  remainingQuantity: number
  totalCost: number
  averagePrice: number | null
  bestAsk: number | null
  isFullyFillable: boolean
  snapshotTime: string
  snapshotSequence: number
}