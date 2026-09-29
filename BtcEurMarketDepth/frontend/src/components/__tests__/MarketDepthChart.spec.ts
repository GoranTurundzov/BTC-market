import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import type { OrderBookSnapshot } from '../../types/order-book-snapshot'

vi.mock('vue-echarts', () => ({
  default: {
    name: 'VChart',
    props: ['option', 'autoresize'],
    template: '<div data-testid="chart"></div>',
  },
}))

import MarketDepthChart from '../MarketDepthChart.vue'

function createSnapshot(levelCount = 2): OrderBookSnapshot {
  return {
    symbol: 'BTC/EUR',
    acquiredAt: '2026-09-06T12:00:00Z',
    sequence: 1,
    bids: Array.from({ length: levelCount }, (_, index) => ({
      price: 100 - index,
      quantity: index + 1,
    })),
    asks: Array.from({ length: levelCount }, (_, index) => ({
      price: 101 + index,
      quantity: index + 1,
    })),
  }
}

function mountChart(snapshot: OrderBookSnapshot | null, isLoading = false) {
  return mount(MarketDepthChart, {
    props: { snapshot, isLoading },
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
  it('renders the order-book chart in ascending price order', () => {
    const wrapper = mountChart(createSnapshot())
    const chart = wrapper.findComponent({ name: 'VChart' })
    const option = chart.props('option') as {
      xAxis: { data: string[] }
      series: Array<{ type: string; data: Array<number | null> }>
    }

    expect(wrapper.text()).toContain('Market depth')
    expect(option.xAxis.data).toEqual([
      '99.00',
      '100.00',
      '',
      '101.00',
      '102.00',
    ])
    expect(option.series[0]?.type).toBe('line')
    expect(option.series[1]?.type).toBe('line')
    expect(option.series[0]?.data).toEqual([2, 1, null, null, null])
    expect(option.series[1]?.data).toEqual([null, null, null, 1, 2])
  })

  it('does not truncate the available order-book levels', () => {
    const wrapper = mountChart(createSnapshot(60))
    const chart = wrapper.findComponent({ name: 'VChart' })
    const option = chart.props('option') as {
      xAxis: { data: string[] }
    }

    expect(option.xAxis.data).toHaveLength(121)
    expect(wrapper.text()).toContain('Showing 60 bid levels and 60 ask levels')
  })

  it('uses a loading skeleton before the order book exists', () => {
    const wrapper = mountChart(null, true)

    expect(wrapper.find('[data-testid="chart"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Waiting for order-book data')
  })
})
