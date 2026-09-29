import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getBuyQuote, getOrderBookHistory } from '../market-api'

describe('market API client', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('requests order-book history with an encoded range', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response('[]', { status: 200 }),
    )

    await getOrderBookHistory(
      '2026-09-29T12:00:00.000Z',
      '2026-09-29T13:00:00.000Z',
    )

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/market/order-book/history?from=2026-09-29T12%3A00%3A00.000Z&to=2026-09-29T13%3A00%3A00.000Z',
    )
  })

  it('adds the selected timestamp to a historical quote request', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response('{}', { status: 200 }),
    )

    await getBuyQuote(0.5, '2026-09-29T13:00:00.000Z')

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/market/quote?quantity=0.5&at=2026-09-29T13%3A00%3A00.000Z',
    )
  })
})
