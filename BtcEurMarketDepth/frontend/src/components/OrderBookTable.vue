<script setup lang="ts">
import { computed, ref } from 'vue'
import type { PriceLevel } from '../types/price-level'

const props = defineProps<{
  title: string
  levels: PriceLevel[]
  side: 'bid' | 'ask'
}>()

const pageSize = 10
const visibleLevelCount = ref(pageSize)

const displayedLevels = computed(() =>
  props.levels.slice(0, visibleLevelCount.value),
)

const hasMoreLevels = computed(() =>
  visibleLevelCount.value < props.levels.length,
)

function loadMore() {
  visibleLevelCount.value += pageSize
}
</script>

<template>
  <section class="overflow-hidden rounded-xl border border-slate-700 bg-slate-900 shadow-lg">
    <div class="border-b border-slate-700 px-5 py-4">
      <h2
        class="text-lg font-semibold"
        :class="side === 'bid' ? 'text-emerald-400' : 'text-rose-400'"
      >
        {{ title }}
      </h2>
    </div>

    <div class="h-124 overflow-y-auto">
      <table class="w-full text-sm">
        <thead class="sticky top-0 z-10 bg-slate-800 text-xs uppercase tracking-wide text-slate-400">
          <tr>
            <th class="px-5 py-3 text-right">Price (EUR)</th>
            <th class="px-5 py-3 text-right">Quantity (BTC)</th>
          </tr>
        </thead>

        <tbody class="divide-y divide-slate-800">
          <tr
            v-for="level in displayedLevels"
            :key="`${level.price}-${level.quantity}`"
            class="transition-colors hover:bg-slate-800"
          >
            <td
              class="px-5 py-3 text-right font-medium"
              :class="side === 'bid' ? 'text-emerald-400' : 'text-rose-400'"
            >
              {{ level.price.toFixed(2) }}
            </td>

            <td class="px-5 py-3 text-right text-slate-300">
              {{ level.quantity }}
            </td>
          </tr>

          <tr v-if="displayedLevels.length === 0">
            <td colspan="2" class="px-5 py-6 text-center text-slate-500">
              No data available
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="flex items-center justify-between border-t border-slate-800 px-5 py-3">
      <span class="text-xs text-slate-500">
        Showing {{ displayedLevels.length }} of {{ levels.length }}
      </span>

      <button
        v-if="hasMoreLevels"
        type="button"
        class="rounded-md bg-slate-700 px-3 py-2 text-xs font-medium text-slate-200 transition hover:bg-slate-600"
        @click="loadMore"
      >
        Load 10 more
      </button>

      <span v-else class="text-xs text-slate-500">
        All levels loaded
      </span>
    </div>
  </section>
</template>