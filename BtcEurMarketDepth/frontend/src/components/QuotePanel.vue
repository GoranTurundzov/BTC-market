<script setup lang="ts">
import type { BuyQuote } from '../types/buy-quote'

defineProps<{
  modelValue: number
  quote: BuyQuote | null
  isLoading: boolean
  errorMessage: string | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: number]
}>()

function handleInput(event: Event) {
  const input = event.target as HTMLInputElement
  emit('update:modelValue', Number(input.value))
}
</script>

<template>
  <section class="rounded-xl border border-slate-700 bg-slate-900 p-5 shadow-lg">
    <h2 class="mb-4 text-lg font-semibold text-slate-100">
      Buy BTC
    </h2>

    <label for="btc-quantity" class="mb-2 block text-sm text-slate-400">
      Amount of BTC
    </label>

    <input
      id="btc-quantity"
      type="number"
      min="0.00000001"
      step="0.00000001"
      :value="modelValue"
      class="w-full rounded-lg border border-slate-600 bg-slate-800 px-4 py-3 text-slate-100 outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-500/30"
      @input="handleInput"
    />

    <p v-if="isLoading" class="mt-4 text-sm text-slate-400">
      Calculating quote...
    </p>

    <p v-else-if="errorMessage" class="mt-4 text-sm text-rose-400">
      {{ errorMessage }}
    </p>

    <div v-else-if="quote" class="mt-5 space-y-3">
      <div class="flex justify-between border-b border-slate-800 pb-3">
        <span class="text-slate-400">Total cost</span>
        <span class="font-semibold text-blue-400">
          €{{ quote.totalCost.toFixed(2) }}
        </span>
      </div>

      <div class="flex justify-between">
        <span class="text-slate-400">Average price</span>
        <span class="text-slate-200">
          {{ quote.averagePrice?.toFixed(2) ?? '—' }} EUR
        </span>
      </div>

      <div class="flex justify-between">
        <span class="text-slate-400">Filled quantity</span>
        <span class="text-slate-200">
          {{ quote.filledQuantity }} BTC
        </span>
      </div>

      <div class="flex justify-between">
        <span class="text-slate-400">Remaining quantity</span>
        <span class="text-slate-200">
          {{ quote.remainingQuantity }} BTC
        </span>
      </div>

      <p
        v-if="!quote.isFullyFillable"
        class="rounded-lg bg-amber-950/50 p-3 text-sm text-amber-300"
      >
        The requested quantity cannot be fully filled with the available asks.
      </p>

      <p
        v-else
        class="rounded-lg bg-emerald-950/50 p-3 text-sm text-emerald-300"
      >
        The requested quantity can be fully filled.
      </p>
    </div>
  </section>
</template>