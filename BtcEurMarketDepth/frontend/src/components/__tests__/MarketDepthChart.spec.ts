import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import type { OrderBookSnapshot } from '../../types/order-book-snapshot'

vi.mock('vue-echarts', () => ({
  default: {
    name: 'VChart',
    props: {
      option: {
        type: Object,
        required: true,
      },
      autoresize: {
        type: Boolean,
        default: false,
      },
    },
    template: '<div data-testid="chart"></div>',
  },
}))

import MarketDepthChart from '../MarketDepthChart.vue'

function createSnapshot(
  bidCount = 2,
  askCount = 2,
): OrderBookSnapshot {
  return {
    symbol: 'BTC/EUR',
    acquiredAt: '2026-09-06T12:00:00Z',
    sequence: 1,
    bids: Array.from(
      { length: bidCount },
      (_, index) => ({
        price: 100 - index,
        quantity: index + 1,
      }),
    ),
    asks: Array.from(
      { length: askCount },
      (_, index) => ({
        price: 101 + index,
        quantity: index + 1,
      }),
    ),
  }
}

function mountChart(snapshot: OrderBookSnapshot) {
  return mount(MarketDepthChart, {
    props: { snapshot },
    global: {
      stubs: {
        VChart: {
          name: 'VChart',
          props: ['option'],
          template: '<div data-testid="chart"></div>',
        },
      },
    },
  })
}

describe('MarketDepthChart', () => {
  it('renders the chart section', () => {
    const wrapper = mountChart(createSnapshot())

    expect(wrapper.text()).toContain('Market Depth')
    expect(wrapper.text()).toContain('Order quantity by price level')
    expect(wrapper.find('[data-testid="chart"]').exists()).toBe(true)
  })

  it('creates bid and ask bars in ascending price order', () => {
    const wrapper = mountChart(createSnapshot())

    const chart = wrapper.findComponent({ name: 'VChart' })

    const option = chart.props('option') as {
      xAxis: {
        data: string[]
      }
      series: Array<{
        data: Array<number | null>
      }>
    }

    expect(option.xAxis.data).toEqual([
      '99.00',
      '100.00',
      '101.00',
      '102.00',
    ])

    expect(option.series[0]!.data).toEqual([
      2,
      1,
      null,
      null,
    ])

    expect(option.series[1]!.data).toEqual([
      null,
      null,
      1,
      2,
    ])
  })

  it('loads another 500 levels when requested', async () => {
    const wrapper = mountChart(createSnapshot(600, 600))

    expect(wrapper.text()).toContain(
      'Showing 500 bid levels and 500 ask levels',
    )

    const loadMoreButton = wrapper
      .findAll('button')
      .find((button) => button.text().includes('Load 500 more'))

    expect(loadMoreButton).toBeDefined()

    await loadMoreButton!.trigger('click')

    expect(wrapper.text()).toContain(
      'Showing 600 bid levels and 600 ask levels',
    )
  })

  it('does not show load-more when all levels are displayed', () => {
    const wrapper = mountChart(createSnapshot(100, 100))

    expect(wrapper.text()).not.toContain('Load 500 more')
    expect(wrapper.text()).not.toContain('Remove 500')
  })
})