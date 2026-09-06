import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import QuotePanel from '../QuotePanel.vue'
import type { BuyQuote } from '../../types/buy-quote'

const quote: BuyQuote = {
  requestedQuantity: 2,
  filledQuantity: 2,
  remainingQuantity: 0,
  totalCost: 210,
  averagePrice: 105,
  bestAsk: 100,
  isFullyFillable: true,
  snapshotTime: '2026-09-06T12:00:00Z',
  snapshotSequence: 1,
}

describe('QuotePanel', () => {
  it('renders the requested quantity', () => {
    const wrapper = mount(QuotePanel, {
      props: {
        modelValue: 2,
        quote: null,
        isLoading: false,
        errorMessage: null,
      },
    })

    const input = wrapper.find('input')

    expect(input.exists()).toBe(true)
    expect(input.element.value).toBe('2')
  })

  it('emits the updated quantity when the input changes', async () => {
    const wrapper = mount(QuotePanel, {
      props: {
        modelValue: 1,
        quote: null,
        isLoading: false,
        errorMessage: null,
      },
    })

    const input = wrapper.find('input')

    await input.setValue('2.5')

    expect(wrapper.emitted('update:modelValue')).toBeTruthy()
    const emittedValues = wrapper.emitted('update:modelValue')

    expect(emittedValues).toBeDefined()
    expect(emittedValues?.[emittedValues.length - 1]).toEqual([2.5])
  })

  it('renders quote details', () => {
    const wrapper = mount(QuotePanel, {
      props: {
        modelValue: 2,
        quote,
        isLoading: false,
        errorMessage: null,
      },
    })

    expect(wrapper.text()).toContain('Total cost')
    expect(wrapper.text()).toContain('Average price')
    expect(wrapper.text()).toContain('Filled quantity')
    expect(wrapper.text()).toContain('Remaining quantity')
    expect(wrapper.text()).toContain('The requested quantity can be fully filled.')
  })

  it('renders the error message', () => {
    const wrapper = mount(QuotePanel, {
      props: {
        modelValue: 2,
        quote: null,
        isLoading: false,
        errorMessage: 'Unable to calculate quote.',
      },
    })

    expect(wrapper.text()).toContain('Unable to calculate quote.')
  })
})