<script setup lang="ts">
import { computed, ref } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { BarChart } from 'echarts/charts'
import {
  DataZoomComponent,
  GridComponent,
  LegendComponent,
  TooltipComponent,
} from 'echarts/components'
import type { EChartsOption } from 'echarts'
import { CanvasRenderer } from 'echarts/renderers'
import type { OrderBookSnapshot } from '../types/order-book-snapshot'

use([
  CanvasRenderer,
  BarChart,
  GridComponent,
  LegendComponent,
  TooltipComponent,
  DataZoomComponent,
])

const LEVELS_PER_LOAD = 500

const visibleLevelCount = ref(LEVELS_PER_LOAD)

const props = defineProps<{
  snapshot: OrderBookSnapshot
}>()

const availableLevelCount = computed(() => {
  return Math.max(
    props.snapshot.bids.length,
    props.snapshot.asks.length,
  )
})

const chartData = computed(() => {
  const levelCount = Math.min(
    visibleLevelCount.value,
    availableLevelCount.value,
  )

  const bids = props.snapshot.bids
    .slice(0, levelCount)
    .reverse()

  const asks = props.snapshot.asks
    .slice(0, levelCount)

  const bidPrices = bids.map((level) => level.price.toFixed(2))
  const askPrices = asks.map((level) => level.price.toFixed(2))

  const categories = [
    ...bidPrices,
    ...askPrices,
  ]

  const bidQuantities = [
    ...bids.map((level) => level.quantity),
    ...asks.map(() => null),
  ]

  const askQuantities = [
    ...bids.map(() => null),
    ...asks.map((level) => level.quantity),
  ]

  return {
    categories,
    bidQuantities,
    askQuantities,
    bidCount: bids.length,
    askCount: asks.length,
  }
})

const hasMoreData = computed(() => {
  return visibleLevelCount.value < availableLevelCount.value
})

const canRemoveData = computed(() => {
  return visibleLevelCount.value > LEVELS_PER_LOAD
})

function loadMoreLevels(): void {
  visibleLevelCount.value = Math.min(
    visibleLevelCount.value + LEVELS_PER_LOAD,
    availableLevelCount.value,
  )
}

function removeLevels(): void {
  visibleLevelCount.value = Math.max(
    visibleLevelCount.value - LEVELS_PER_LOAD,
    LEVELS_PER_LOAD,
  )
}

const chartOption = computed<EChartsOption>(() => ({
  animation: false,

  tooltip: {
    trigger: 'axis',
    axisPointer: {
      type: 'shadow',
    },
    valueFormatter: (value) =>
      typeof value === 'number'
        ? `${value.toFixed(8)} BTC`
        : '-',
  },

  legend: {
    data: ['Bids', 'Asks'],
    textStyle: {
      color: '#cbd5e1',
    },
  },

  grid: {
    left: '5%',
    right: '4%',
    bottom: '18%',
    top: '12%',
    containLabel: true,
  },

  dataZoom: [
    {
      type: 'inside',
      xAxisIndex: 0,
      filterMode: 'none',
      zoomOnMouseWheel: true,
      moveOnMouseMove: true,
      moveOnMouseWheel: true,
    },
    {
      type: 'slider',
      xAxisIndex: 0,
      filterMode: 'none',
      height: 20,
      bottom: 10,
      start: 0,
      end: 35,
      borderColor: '#475569',
      backgroundColor: '#1e293b',
      fillerColor: 'rgba(59, 130, 246, 0.25)',
      handleStyle: {
        color: '#3b82f6',
      },
      textStyle: {
        color: '#cbd5e1',
      },
    },
  ],

  xAxis: {
    type: 'category',
    data: chartData.value.categories,
    name: 'Price (EUR)',
    nameTextStyle: {
      color: '#94a3b8',
    },
    axisLabel: {
      color: '#94a3b8',
      rotate: 45,
      hideOverlap: true,
    },
    axisLine: {
      lineStyle: {
        color: '#475569',
      },
    },
    splitLine: {
      show: false,
    },
  },

  yAxis: {
    type: 'value',
    name: 'Quantity (BTC)',
    nameTextStyle: {
      color: '#94a3b8',
    },
    axisLabel: {
      color: '#94a3b8',
    },
    axisLine: {
      lineStyle: {
        color: '#475569',
      },
    },
    splitLine: {
      lineStyle: {
        color: '#1e293b',
      },
    },
  },

  series: [
    {
      name: 'Bids',
      type: 'bar',
      data: chartData.value.bidQuantities,
      barMaxWidth: 10,
      itemStyle: {
        color: '#34d399',
      },
    },
    {
      name: 'Asks',
      type: 'bar',
      data: chartData.value.askQuantities,
      barMaxWidth: 10,
      itemStyle: {
        color: '#fb7185',
      },
    },
  ],
}))
</script>

<template>
  <section class="rounded-xl border border-slate-700 bg-slate-900 p-5">
    <div class="mb-4">
      <h2 class="text-lg font-semibold text-slate-100">
        Market Depth
      </h2>

      <p class="text-sm text-slate-400">
        Order quantity by price level
      </p>
    </div>

    <VChart
      style="height: 28rem; width: 100%"
      :option="chartOption"
      autoresize
    />

    <div class="mt-4 flex items-center justify-between">
      <p class="text-sm text-slate-400">
        Showing {{ chartData.bidCount }} bid levels and
        {{ chartData.askCount }} ask levels
      </p>

      <div class="flex gap-2">
        <button
          v-if="canRemoveData"
          type="button"
          class="rounded-md bg-slate-700 px-4 py-2 text-sm font-medium text-slate-100 transition hover:bg-slate-600"
          @click="removeLevels"
        >
          Remove 500
        </button>

        <button
          v-if="hasMoreData"
          type="button"
          class="rounded-md bg-slate-700 px-4 py-2 text-sm font-medium text-slate-100 transition hover:bg-slate-600"
          @click="loadMoreLevels"
        >
          Load 500 more
        </button>
      </div>
    </div>
  </section>
</template>