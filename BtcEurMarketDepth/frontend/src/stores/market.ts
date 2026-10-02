import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import type { HubConnection } from '@microsoft/signalr'
import { getBuyQuote, getOrderBook, getOrderBookHistory } from '../services/market-api'
import { createMarketHub } from '../services/market-hub'
import type { BuyQuote } from '../types/buy-quote'
import type { OrderBookSnapshot } from '../types/order-book-snapshot'

export type ChartRange = 'CURRENT' | '1H' | '1D' | '1W' | '1M' | '1Y' | 'YTD' | 'ALL' | 'CUSTOM'

export type PresetChartRange = Exclude<ChartRange, 'CUSTOM'>

const rangeDurations: Record<Exclude<PresetChartRange, 'CURRENT' | 'ALL' | 'YTD'>, number> = {
  '1H': 60 * 60 * 1000,
  '1D': 24 * 60 * 60 * 1000,
  '1W': 7 * 24 * 60 * 60 * 1000,
  '1M': 30 * 24 * 60 * 60 * 1000,
  '1Y': 365 * 24 * 60 * 60 * 1000,
}

const maximumLiveChartSnapshots = 5_000
const presetHistoryCacheLifetimeMilliseconds = 30_000

function getRangeStart(range: Exclude<PresetChartRange, 'CURRENT'>, end: Date): Date {
  if (range === 'ALL') {
    return new Date(0)
  }

  if (range === 'YTD') {
    return new Date(end.getFullYear(), 0, 1)
  }

  return new Date(end.getTime() - rangeDurations[range])
}

function getErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}

export const useMarketStore = defineStore('market', () => {
  const liveOrderBook = ref<OrderBookSnapshot | null>(null)
  const chartSnapshots = ref<OrderBookSnapshot[]>([])
  const chartSnapshotOverride = ref<OrderBookSnapshot | null>(null)
  const historicalSnapshots = ref<OrderBookSnapshot[]>([])
  const selectedHistoricalTime = ref('')
  const chartRange = ref<ChartRange>('CURRENT')

  const isLoading = ref(true)
  const isHistoryLoading = ref(false)
  const isChartLoading = ref(false)
  const errorMessage = ref<string | null>(null)
  const historyErrorMessage = ref<string | null>(null)

  const requestedQuantity = ref('')
  const quote = ref<BuyQuote | null>(null)
  const isQuoteLoading = ref(false)
  const quoteErrorMessage = ref<string | null>(null)

  let marketHubConnection: HubConnection | null = null
  let quoteRequestId = 0
  const historyWindows: Array<{
    from: number
    to: number
    snapshots: OrderBookSnapshot[]
    cachedAt: number
    range?: PresetChartRange
  }> = []

  const selectedHistoricalSnapshot = computed(() =>
    historicalSnapshots.value.find(
      (snapshot) => snapshot.acquiredAt === selectedHistoricalTime.value,
    ) ?? null,
  )

  const displayedOrderBook = computed(() =>
    selectedHistoricalSnapshot.value ?? liveOrderBook.value,
  )

  const chartSnapshot = computed(() =>
    selectedHistoricalSnapshot.value
    ?? chartSnapshotOverride.value
    ?? liveOrderBook.value,
  )

  const isHistorical = computed(() =>
    selectedHistoricalSnapshot.value !== null,
  )

  function receiveLiveSnapshot(snapshot: OrderBookSnapshot) {
    liveOrderBook.value = snapshot

    if (!selectedHistoricalSnapshot.value) {
      chartSnapshotOverride.value = snapshot
    }

    chartSnapshots.value = [
      ...chartSnapshots.value,
      snapshot,
    ].slice(-maximumLiveChartSnapshots)
  }

  function getCachedHistory(
    from: Date,
    to: Date,
    range?: PresetChartRange,
  ): OrderBookSnapshot[] | null {
    const fromMilliseconds = from.getTime()
    const toMilliseconds = to.getTime()
    const cachedWindow = historyWindows.find((window) =>
      window.from <= fromMilliseconds && (
        window.to >= toMilliseconds
        || (
          range !== undefined
          && window.range === range
          && Date.now() - window.cachedAt <= presetHistoryCacheLifetimeMilliseconds
        )
      ),
    )

    if (!cachedWindow) {
      return null
    }

    return cachedWindow.snapshots.filter((snapshot) => {
      const acquiredAt = new Date(snapshot.acquiredAt).getTime()
      return acquiredAt >= fromMilliseconds && acquiredAt <= toMilliseconds
    })
  }

  function cacheHistory(
    from: Date,
    to: Date,
    snapshots: OrderBookSnapshot[],
    range?: PresetChartRange,
  ) {
    historyWindows.push({
      from: from.getTime(),
      to: to.getTime(),
      snapshots,
      cachedAt: Date.now(),
      range,
    })
  }

  function applyChartSnapshots(snapshots: OrderBookSnapshot[]) {
    chartSnapshots.value = snapshots
    chartSnapshotOverride.value =
      snapshots[snapshots.length - 1]
      ?? liveOrderBook.value
  }

  async function loadChartRange(
    range: Exclude<ChartRange, 'CUSTOM'> = chartRange.value === 'CUSTOM'
      ? 'CURRENT'
      : chartRange.value,
  ) {
    chartRange.value = range

    if (range === 'CURRENT') {
      chartSnapshots.value = []
      historicalSnapshots.value = []
      selectedHistoricalTime.value = ''
      historyErrorMessage.value = null
      chartSnapshotOverride.value = null
      isChartLoading.value = false
      return
    }

    isChartLoading.value = true

    try {
      const to = new Date()
      const from = getRangeStart(range, to)
      const cachedSnapshots = getCachedHistory(from, to, range)
      const snapshots = cachedSnapshots ?? await getOrderBookHistory(
        from.toISOString(),
        to.toISOString(),
      )

      if (cachedSnapshots === null) {
        cacheHistory(from, to, snapshots, range)
      }

      historicalSnapshots.value = []
      selectedHistoricalTime.value = ''
      historyErrorMessage.value = null
      applyChartSnapshots(snapshots)
    } catch (error) {
      applyChartSnapshots([])
      errorMessage.value = getErrorMessage(
        error,
        'Unable to load chart history.',
      )
    } finally {
      isChartLoading.value = false
    }
  }

  async function loadQuote() {
  const requestId = ++quoteRequestId
  const rawQuantity = requestedQuantity.value.trim()

  quoteErrorMessage.value = null

  if (rawQuantity === '') {
    quote.value = null
    isQuoteLoading.value = false
    return
  }

  const quantity = Number(rawQuantity)

  if (!Number.isFinite(quantity) || quantity <= 0) {
    quote.value = null
    quoteErrorMessage.value = 'Enter a quantity greater than zero.'
    isQuoteLoading.value = false
    return
  }

  if (!displayedOrderBook.value) {
    quote.value = null
    isQuoteLoading.value = false
    return
  }

  isQuoteLoading.value = true

  try {
    const nextQuote = await getBuyQuote(
      quantity,
      isHistorical.value
        ? displayedOrderBook.value.acquiredAt
        : undefined,
    )

    if (requestId === quoteRequestId) {
      quote.value = nextQuote
    }
  } catch (error) {
    if (requestId === quoteRequestId) {
      quoteErrorMessage.value = getErrorMessage(
        error,
        'Unable to calculate quote.',
      )
    }
  } finally {
    if (requestId === quoteRequestId) {
      isQuoteLoading.value = false
    }
  }
}

  async function connect() {
    if (marketHubConnection) {
      return
    }

    isLoading.value = true
    errorMessage.value = null

    try {
      // The current snapshot is the critical path. Render it as soon as it
      // arrives instead of waiting for the historical chart query.
      const snapshot = await getOrderBook()

      receiveLiveSnapshot(snapshot)
      isLoading.value = false

      marketHubConnection = createMarketHub(receiveLiveSnapshot)

      // Neither the SignalR handshake nor the chart history should block the
      // first usable render of the current order book.
      void marketHubConnection.start().catch((error: unknown) => {
        errorMessage.value = getErrorMessage(
          error,
          'Unable to connect to live market updates.',
        )
      })

    } catch (error) {
      errorMessage.value = getErrorMessage(
        error,
        'Unable to connect to the market feed.',
      )
      isLoading.value = false
    }
  }

  async function disconnect() {
    await marketHubConnection?.stop()
    marketHubConnection = null
  }

  async function searchHistory(from: string, to: string) {
    isHistoryLoading.value = true
    historyErrorMessage.value = null
    chartRange.value = 'CUSTOM'

    try {
      const fromDate = new Date(from)
      const toDate = new Date(to)
      const cachedSnapshots = getCachedHistory(fromDate, toDate)
      const snapshots = cachedSnapshots ?? await getOrderBookHistory(from, to)

      if (cachedSnapshots === null) {
        cacheHistory(fromDate, toDate, snapshots)
      }

      historicalSnapshots.value = snapshots
      selectedHistoricalTime.value =
        historicalSnapshots.value[historicalSnapshots.value.length - 1]?.acquiredAt ?? ''

      applyChartSnapshots(historicalSnapshots.value)

      if (historicalSnapshots.value.length === 0) {
        historyErrorMessage.value = 'No snapshots were found for that period.'
      }
    } catch (error) {
      historicalSnapshots.value = []
      selectedHistoricalTime.value = ''
      historyErrorMessage.value = getErrorMessage(
        error,
        'Unable to load history.',
      )
    } finally {
      isHistoryLoading.value = false
    }
  }

  function returnToLive() {
    historicalSnapshots.value = []
    selectedHistoricalTime.value = ''
    historyErrorMessage.value = null
    chartRange.value = 'CURRENT'
    chartSnapshots.value = []
    chartSnapshotOverride.value = null
  }

  return {
    liveOrderBook,
    chartSnapshots,
    chartSnapshot,
    historicalSnapshots,
    selectedHistoricalTime,
    chartRange,
    isLoading,
    isHistoryLoading,
    isChartLoading,
    errorMessage,
    historyErrorMessage,
    requestedQuantity,
    quote,
    isQuoteLoading,
    quoteErrorMessage,
    displayedOrderBook,
    isHistorical,
    connect,
    disconnect,
    loadChartRange,
    loadQuote,
    searchHistory,
    returnToLive,
  }
})
