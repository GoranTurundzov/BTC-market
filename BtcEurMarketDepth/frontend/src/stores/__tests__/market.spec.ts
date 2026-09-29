import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import type { OrderBookSnapshot } from '../../types/order-book-snapshot'

const mocks = vi.hoisted(() => ({
  getBuyQuote: vi.fn(),
  getOrderBookHistory: vi.fn(),
}))

vi.mock('../../services/market-api', () => ({
  getBuyQuote: mocks.getBuyQuote,
  getOrderBook: vi.fn(),
  getOrderBookHistory: mocks.getOrderBookHistory,
}))

vi.mock('../../services/market-hub', () => ({
  createMarketHub: vi.fn(),
}))

import { useMarketStore } from '../market'

const snapshot: OrderBookSnapshot = {
  symbol: 'BTC/EUR',
  acquiredAt: '2026-09-29T12:00:00Z',
  sequence: 1,
  bids: [{ price: 100, quantity: 1 }],
  asks: [{ price: 101, quantity: 1 }],
}

describe('market store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    mocks.getBuyQuote.mockReset()
    mocks.getOrderBookHistory.mockReset()
  })

  it('does not request a quote while the quantity is empty', async () => {
    const store = useMarketStore()

    store.requestedQuantity = ''
    await store.loadQuote()

    expect(mocks.getBuyQuote).not.toHaveBeenCalled()
    expect(store.quote).toBeNull()
  })

  it('owns chart-range history loading', async () => {
    mocks.getOrderBookHistory.mockResolvedValue([snapshot])

    const store = useMarketStore()
    await store.loadChartRange('1D')

    expect(store.chartRange).toBe('1D')
    expect(store.chartSnapshots).toEqual([snapshot])
    expect(mocks.getOrderBookHistory).toHaveBeenCalledTimes(1)
  })
})
