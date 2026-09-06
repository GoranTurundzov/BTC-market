import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import OrderBookTable from '../OrderBookTable.vue'
import type { PriceLevel } from '../../types/price-level'

const levels: PriceLevel[] = Array.from(
  { length: 12 },
  (_, index) => ({
    price: 68_500 - index,
    quantity: index + 1,
  }),
)

describe('OrderBookTable', () => {
  it('renders the title and the first ten levels', () => {
    const wrapper = mount(OrderBookTable, {
      props: {
        title: 'Bids',
        levels,
        side: 'bid',
      },
    })

    expect(wrapper.text()).toContain('Bids')
    expect(wrapper.text()).toContain('68500.00')
    expect(wrapper.text()).toContain('68491.00')
    expect(wrapper.text()).not.toContain('68490.00')
    expect(wrapper.text()).toContain('Showing 10 of 12')
  })

  it('loads ten more levels when requested', async () => {
    const wrapper = mount(OrderBookTable, {
      props: {
        title: 'Asks',
        levels,
        side: 'ask',
      },
    })

    const loadMoreButton = wrapper.find('button')

    expect(loadMoreButton.exists()).toBe(true)
    expect(loadMoreButton.text()).toContain('Load 10 more')

    await loadMoreButton.trigger('click')

    expect(wrapper.text()).toContain('68490.00')
    expect(wrapper.text()).toContain('Showing 12 of 12')
  })

  it('does not render the load-more button when all levels are visible', () => {
    const wrapper = mount(OrderBookTable, {
      props: {
        title: 'Bids',
        levels: levels.slice(0, 5),
        side: 'bid',
      },
    })

    expect(wrapper.find('button').exists()).toBe(false)
  })
})