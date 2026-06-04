import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'

import FormField from './FormField.vue'

describe('FormField', () => {
  it('renders label and binds modelValue', () => {
    const wrapper = mount(FormField, {
      props: {
        label: 'Email',
        icon: 'mail',
        modelValue: 'test@example.com',
        'onUpdate:modelValue': () => {},
      },
    })

    expect(wrapper.text()).toContain('Email')
    const input = wrapper.get('input')
    expect((input.element as HTMLInputElement).value).toBe('test@example.com')
  })

  it('shows password toggle for password fields', async () => {
    const wrapper = mount(FormField, {
      props: {
        label: 'Hasło',
        type: 'password',
        icon: 'lock',
        modelValue: '',
        'onUpdate:modelValue': () => {},
      },
    })

    expect(wrapper.get('input').attributes('type')).toBe('password')
    await wrapper.get('button[type="button"]').trigger('click')
    expect(wrapper.get('input').attributes('type')).toBe('text')
  })
})
