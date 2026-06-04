<script setup lang="ts" generic="T">
  /**
   * Generyczna lista — renderuje items przez scoped slot (np. filtry, siatki).
   * @example
   * <DataList :items="genres" item-key="name" list-class="check-list">
   *   <template #default="{ item }">…</template>
   * </DataList>
   */
  const props = withDefaults(
    defineProps<{
      items: T[]
      /** Pole obiektu jako :key (domyślnie indeks). */
      itemKey?: keyof T
      tag?: string
      itemTag?: string
      listClass?: string | string[] | Record<string, boolean>
      itemClass?: string | string[] | Record<string, boolean>
    }>(),
    {
      tag: 'ul',
      itemTag: 'li',
    },
  )

  function resolveKey(item: T, index: number): string | number {
    if (props.itemKey != null && item != null && typeof item === 'object') {
      const value = (item as Record<string | number | symbol, unknown>)[props.itemKey as keyof T]
      if (typeof value === 'string' || typeof value === 'number') return value
    }
    return index
  }
</script>

<template>
  <component :is="tag" :class="listClass">
    <component
      v-for="(item, index) in items"
      :is="itemTag"
      :key="resolveKey(item, index)"
      :class="itemClass"
    >
      <slot :item="item" :index="index" />
    </component>
  </component>
</template>
