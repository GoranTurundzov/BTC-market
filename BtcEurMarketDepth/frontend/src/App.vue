<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import QuotePanel from './components/QuotePanel.vue'
import MarketDepthChart from './components/MarketDepthChart.vue'
import { useMarketStore, type ChartRange, type PresetChartRange } from './stores/market'

const market = useMarketStore()

const chartRanges: ChartRange[] = ['CURRENT', '1H', '1D', '1W', '1M', '1Y', 'YTD', 'ALL', 'CUSTOM']

const currentVisibleLevels = ref(500)

function toDateTimeLocal(value: Date): string {
  const offset = value.getTimezoneOffset() * 60_000
  return new Date(value.getTime() - offset).toISOString().slice(0, 16)
}

const historyFrom = toDateTimeLocal(new Date(Date.now() - 60 * 60 * 1000))
const historyTo = toDateTimeLocal(new Date())

function searchHistory() {
  const from = new Date(historyFrom)
  const to = new Date(historyTo)

  if (Number.isNaN(from.getTime()) || Number.isNaN(to.getTime())) {
    market.historyErrorMessage = 'Enter both history timestamps.'
    return
  }

  if (from > to) {
    market.historyErrorMessage = 'The From timestamp must be before To.'
    return
  }

  void market.searchHistory(from.toISOString(), to.toISOString())
}

function selectChartRange(range: ChartRange) {
  if (range === 'CUSTOM') {
    market.chartRange = 'CUSTOM'
    return
  }

  void market.loadChartRange(range as PresetChartRange)
}

function adjustCurrentVisibleLevels(amount: number) {
  currentVisibleLevels.value += amount
}

watch(
  [() => market.requestedQuantity, () => market.displayedOrderBook],
  () => void market.loadQuote(),
)

onMounted(() => {
  void market.connect()
})

onBeforeUnmount(() => {
  void market.disconnect()
})
</script>

<template>
  <main class="min-h-screen bg-slate-950 px-4 py-8 text-slate-100 sm:px-8">
    <div class="mx-auto max-w-7xl">
      <header class="mb-6 flex items-center justify-between">
        <div>
          <p class="mb-1 text-sm font-medium uppercase tracking-widest text-blue-400">
            Market overview
          </p>
          <h1 class="text-3xl font-bold tracking-tight">BTC/EUR Market</h1>
          <p class="mt-1 text-sm text-slate-400">
            {{ market.isHistorical ? 'Historical snapshot' : 'Live market data' }}
            <span v-if="market.displayedOrderBook">
              · {{ new Date(market.displayedOrderBook.acquiredAt).toLocaleString() }}
            </span>
          </p>
        </div>

        <span
          class="rounded-full px-3 py-1 text-xs font-medium"
          :class="
            market.isLoading ? 'bg-slate-800 text-slate-400' : 'bg-emerald-950 text-emerald-400'
          "
        >
          {{ market.isLoading ? 'Connecting' : 'Live' }}
        </span>
      </header>

      <div
        v-if="market.errorMessage && !market.displayedOrderBook"
        class="mb-6 rounded-xl border border-rose-900 bg-rose-950/40 p-4 text-rose-300"
      >
        {{ market.errorMessage }}
      </div>

      <section class="mb-6 rounded-xl border border-slate-700 bg-slate-900 p-5 shadow-lg">
        <div class="mb-4 flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 class="text-lg font-semibold">Price chart</h2>
            <p class="text-sm text-slate-400">Order-book levels from the selected period</p>
          </div>

          <div class="flex rounded-lg bg-slate-800 p-1">
            <button
              v-for="range in chartRanges"
              :key="range"
              type="button"
              class="rounded-md px-3 py-1.5 text-xs font-medium transition"
              :class="
                market.chartRange === range
                  ? 'bg-slate-600 text-slate-100 shadow-sm'
                  : 'text-slate-400 hover:text-slate-100'
              "
              @click="selectChartRange(range)"
            >
              {{ range === 'CURRENT' ? 'Current' : range === 'CUSTOM' ? 'Custom' : range }}
            </button>
          </div>
        </div>

        <div
          v-if="market.chartRange === 'CUSTOM'"
          class="mb-4 rounded-lg border border-slate-700 bg-slate-800/60 p-4"
        >
          <div class="grid gap-4 md:grid-cols-[1fr_1fr_auto]">
            <label class="text-sm text-slate-400">
              From
              <input
                v-model="historyFrom"
                type="datetime-local"
                class="mt-1 block w-full rounded-md border border-slate-600 bg-slate-900 px-3 py-2 text-slate-100"
              />
            </label>

            <label class="text-sm text-slate-400">
              To
              <input
                v-model="historyTo"
                type="datetime-local"
                class="mt-1 block w-full rounded-md border border-slate-600 bg-slate-900 px-3 py-2 text-slate-100"
              />
            </label>

            <button
              type="button"
              class="self-end rounded-md bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-500 disabled:opacity-50"
              :disabled="market.isHistoryLoading"
              @click="searchHistory"
            >
              {{ market.isHistoryLoading ? 'Searching...' : 'Search' }}
            </button>
          </div>

          <p v-if="market.historyErrorMessage" class="mt-3 text-sm text-rose-300">
            {{ market.historyErrorMessage }}
          </p>
        </div>

        <div
          v-if="market.chartRange === 'CURRENT'"
          class="mb-4 flex items-center justify-end gap-2"
        >
          <span class="mr-2 text-xs text-slate-400">
            {{ currentVisibleLevels }} levels per side
          </span>
          <button
            type="button"
            class="rounded-md bg-slate-800 px-3 py-1.5 text-sm text-slate-200 hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-40"
            :disabled="currentVisibleLevels <= 500"
            @click="adjustCurrentVisibleLevels(-500)"
          >
            −500
          </button>
          <button
            type="button"
            class="rounded-md bg-slate-800 px-3 py-1.5 text-sm text-slate-200 hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-40"
            :disabled="
              currentVisibleLevels >=
              Math.max(
                500,
                Math.max(
                  market.liveOrderBook?.bids.length ?? 0,
                  market.liveOrderBook?.asks.length ?? 0,
                ),
              )
            "
            @click="adjustCurrentVisibleLevels(500)"
          >
            +500
          </button>
        </div>

        <MarketDepthChart
          :snapshot="market.chartSnapshot"
          :is-loading="market.isLoading"
          :visible-level-limit="market.chartRange === 'CURRENT' ? currentVisibleLevels : undefined"
        />
      </section>

      <QuotePanel
        v-model="market.requestedQuantity"
        :quote="market.quote"
        :is-loading="market.isQuoteLoading"
        :error-message="market.quoteErrorMessage"
      />
    </div>
  </main>
</template>
