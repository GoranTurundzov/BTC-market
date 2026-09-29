<script setup lang="ts">
import { computed } from 'vue'
import VChart from 'vue-echarts'
import { LineChart } from 'echarts/charts'
import { GridComponent, LegendComponent, TooltipComponent } from 'echarts/components'
import type { EChartsOption } from 'echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import type { OrderBookSnapshot } from '../types/order-book-snapshot'

use([CanvasRenderer, LineChart, GridComponent, LegendComponent, TooltipComponent])

const props = defineProps<{
  snapshot: OrderBookSnapshot | null
  isLoading?: boolean
  visibleLevelLimit?: number
}>()

const chartData = computed(() => {
  const levelLimit = props.visibleLevelLimit
  const bids = [...(props.snapshot?.bids ?? [])].slice(0, levelLimit).reverse()
  const asks = [...(props.snapshot?.asks ?? [])].slice(0, levelLimit)

  return {
    categories: [
      ...bids.map((level) => level.price.toFixed(2)),
      '',
      ...asks.map((level) => level.price.toFixed(2)),
    ],
    bidQuantities: [...bids.map((level) => level.quantity), null, ...asks.map(() => null)],
    askQuantities: [...bids.map(() => null), null, ...asks.map((level) => level.quantity)],
    bidCount: bids.length,
    askCount: asks.length,
  }
})

const chartOption = computed<EChartsOption>(() => ({
  animation: false,
  tooltip: {
    trigger: 'axis',
    axisPointer: { type: 'line' },
    valueFormatter: (value) => (typeof value === 'number' ? `${value.toFixed(8)} BTC` : '-'),
  },
  legend: {
    data: ['Bids', 'Asks'],
    textStyle: { color: '#cbd5e1' },
  },
  grid: {
    left: '6%',
    right: '4%',
    bottom: '16%',
    top: '12%',
    containLabel: true,
  },
  xAxis: {
    type: 'category',
    data: chartData.value.categories,
    name: 'BTC price (increasing)',
    nameLocation: 'middle',
    nameGap: 42,
    nameTextStyle: { color: '#94a3b8' },
    axisLabel: {
      color: '#94a3b8',
      rotate: 45,
      hideOverlap: true,
    },
    axisLine: { lineStyle: { color: '#475569' } },
  },
  yAxis: {
    type: 'value',
    name: 'Amount of BTC available',
    nameLocation: 'middle',
    nameGap: 52,
    nameTextStyle: { color: '#94a3b8' },
    axisLabel: { color: '#94a3b8' },
    splitLine: { lineStyle: { color: '#1e293b' } },
  },
  series: [
    {
      name: 'Bids',
      type: 'line',
      showSymbol: false,
      connectNulls: false,
      data: chartData.value.bidQuantities,
      lineStyle: { color: '#22c55e', width: 2 },
      areaStyle: { color: 'rgba(34, 197, 94, 0.16)' },
    },
    {
      name: 'Asks',
      type: 'line',
      showSymbol: false,
      connectNulls: false,
      data: chartData.value.askQuantities,
      lineStyle: { color: '#f43f5e', width: 2 },
      areaStyle: { color: 'rgba(244, 63, 94, 0.16)' },
    },
  ],
}))
</script>

<template>
  <section class="rounded-xl border border-slate-700 bg-slate-900 p-5 shadow-lg">
    <div class="mb-4 flex items-start justify-between gap-4">
      <div>
        <h2 class="text-lg font-semibold text-slate-100">Market depth</h2>
        <p v-if="snapshot" class="mt-1 text-2xl font-semibold text-slate-100">
          {{ snapshot.symbol }}
          <span class="ml-2 text-sm font-medium text-slate-400">
            {{ new Date(snapshot.acquiredAt).toLocaleString() }}
          </span>
        </p>
        <p v-else class="mt-2 text-sm text-slate-400">Waiting for order-book data</p>
      </div>

      <span class="text-xs text-slate-400">Bid / ask depth</span>
    </div>

    <div v-if="isLoading" class="h-80 animate-pulse rounded-lg bg-slate-800" />

    <VChart
      v-else-if="snapshot"
      style="height: 20rem; width: 100%"
      :option="chartOption"
      autoresize
    />

    <div v-else class="flex h-80 items-center justify-center text-sm text-slate-400">
      No order-book data available for this period.
    </div>

    <p v-if="snapshot" class="mt-3 text-sm text-slate-400">
      Showing {{ chartData.bidCount }} bid levels and {{ chartData.askCount }} ask levels
      <span v-if="visibleLevelLimit" class="text-slate-500">
        ({{ snapshot.bids.length }} bids / {{ snapshot.asks.length }} asks available)
      </span>
    </p>
  </section>
</template>
