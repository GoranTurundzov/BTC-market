<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import OrderBookTable from './components/OrderBookTable.vue'
import QuotePanel from './components/QuotePanel.vue'
import { getBuyQuote, getOrderBook } from './services/market-api'
import type { BuyQuote } from './types/buy-quote'
import type { OrderBookSnapshot } from './types/order-book-snapshot.ts'
import { createMarketHub } from './services/market-hub'
import type { HubConnection } from '@microsoft/signalr'
import MarketDepthChart from './components/MarketDepthChart.vue'

const orderBook = ref<OrderBookSnapshot | null>(null)
const isLoading = ref(true)
const errorMessage = ref<string | null>(null)

const requestedQuantity = ref(1)
const quote = ref<BuyQuote | null>(null)
const isQuoteLoading = ref(false)
const quoteErrorMessage = ref<string | null>(null)
let marketHubConnection: HubConnection | null = null

async function loadQuote(quantity: number) {
  if (!Number.isFinite(quantity) || quantity <= 0) {
    quote.value = null
    quoteErrorMessage.value = 'Enter a quantity greater than zero.'
    return
  }

  isQuoteLoading.value = true
  quoteErrorMessage.value = null

  try {
    quote.value = await getBuyQuote(quantity)
  } catch (error) {
    quote.value = null
    quoteErrorMessage.value = error instanceof Error ? error.message : 'Unknown error occurred.'
  } finally {
    isQuoteLoading.value = false
  }
}

watch(requestedQuantity, loadQuote, { immediate: true })
watch(orderBook, (snapshot) => {
  if (snapshot) {
    void loadQuote(requestedQuantity.value)
  }
})

onMounted(async () => {
  try {
    orderBook.value = await getOrderBook()

    marketHubConnection = createMarketHub((snapshot) => {
      orderBook.value = snapshot
    })

    await marketHubConnection.start()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unknown error occurred.'
  } finally {
    isLoading.value = false
  }
})

onBeforeUnmount(async () => {
  await marketHubConnection?.stop()
})
</script>

<template>
  <main class="min-h-screen bg-slate-950 px-4 py-8 text-slate-100 sm:px-8">
    <div class="mx-auto max-w-6xl">
      <header class="mb-8">
        <p class="mb-2 text-sm font-medium uppercase tracking-widest text-blue-400">
          Market overview
        </p>

        <h1 class="text-3xl font-bold tracking-tight sm:text-4xl">BTC/EUR Market Depth</h1>

        <p v-if="orderBook" class="mt-2 text-sm text-slate-400">
          Last snapshot: {{ new Date(orderBook.acquiredAt).toLocaleString() }}
        </p>
      </header>

      <div
        v-if="isLoading"
        class="rounded-xl border border-slate-700 bg-slate-900 p-8 text-center text-slate-400"
      >
        Loading order book...
      </div>

      <div
        v-else-if="errorMessage"
        class="rounded-xl border border-rose-900 bg-rose-950/40 p-8 text-center text-rose-300"
      >
        {{ errorMessage }}
      </div>

      <section v-else-if="orderBook" class="space-y-6">

        <MarketDepthChart :snapshot="orderBook" />
        
        <div class="grid gap-6 lg:grid-cols-2">
          <OrderBookTable title="Bids" :levels="orderBook.bids" side="bid" />
          <OrderBookTable title="Asks" :levels="orderBook.asks" side="ask" />
        </div>

        <QuotePanel
          v-model="requestedQuantity"
          :quote="quote"
          :is-loading="isQuoteLoading"
          :error-message="quoteErrorMessage"
        />
      </section>
    </div>
  </main>
</template>
