<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getOrderBook } from './services/marketApi'
import type { OrderBookSnapshot } from './types/market'

const orderBook = ref<OrderBookSnapshot | null>(null)
const isLoading = ref(true)
const errorMessage = ref<string | null>(null)

onMounted(async () => {
  try {
    orderBook.value = await getOrderBook()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unknown error occurred.'
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <main>
    <h1>BTC/EUR Market Depth</h1>

    <p v-if="isLoading">Loading order book...</p>

    <p v-else-if="errorMessage" class="error">
      {{ errorMessage }}
    </p>

    <section v-else-if="orderBook" class="order-book">
      <div>
        <h2>Bids</h2>

        <table>
          <thead>
            <tr>
              <th>Price (EUR)</th>
              <th>Quantity (BTC)</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="bid in orderBook.bids" :key="`${bid.price}-${bid.quantity}`">
              <td>{{ bid.price.toFixed(2) }}</td>
              <td>{{ bid.quantity }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div>
        <h2>Asks</h2>

        <table>
          <thead>
            <tr>
              <th>Price (EUR)</th>
              <th>Quantity (BTC)</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="ask in orderBook.asks" :key="`${ask.price}-${ask.quantity}`">
              <td>{{ ask.price.toFixed(2) }}</td>
              <td>{{ ask.quantity }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </main>
</template>

<style scoped></style>