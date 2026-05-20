<script setup lang="ts">
import type { Pager } from '~/features/orden-surtimiento/model/common'

const props = defineProps<{ pager: Pager }>()
const emit = defineEmits<{ (e: 'change', page: number): void }>()

const pages = computed(() => {
  const result: number[] = []
  for (let i = props.pager.startPage; i <= props.pager.endPage; i++) result.push(i)
  return result
})
</script>

<template>
  <div class="d-flex align-center justify-space-between flex-wrap gap-2 mt-2">
    <p class="text-caption text-medium-emphasis">
      {{ pager.totalItems }} registros · Página {{ pager.currentPage }} de {{ pager.totalPages }}
    </p>
    <div class="d-flex gap-1">
      <v-btn
        icon="mdi-page-first"
        size="x-small"
        variant="text"
        :disabled="pager.currentPage === 1"
        @click="emit('change', 1)"
      />
      <v-btn
        icon="mdi-chevron-left"
        size="x-small"
        variant="text"
        :disabled="pager.currentPage === 1"
        @click="emit('change', pager.currentPage - 1)"
      />
      <v-btn
        v-for="p in pages"
        :key="p"
        size="x-small"
        :variant="p === pager.currentPage ? 'flat' : 'text'"
        :color="p === pager.currentPage ? 'primary' : undefined"
        @click="emit('change', p)"
      >
        {{ p }}
      </v-btn>
      <v-btn
        icon="mdi-chevron-right"
        size="x-small"
        variant="text"
        :disabled="pager.currentPage === pager.totalPages"
        @click="emit('change', pager.currentPage + 1)"
      />
      <v-btn
        icon="mdi-page-last"
        size="x-small"
        variant="text"
        :disabled="pager.currentPage === pager.totalPages"
        @click="emit('change', pager.totalPages)"
      />
    </div>
  </div>
</template>
