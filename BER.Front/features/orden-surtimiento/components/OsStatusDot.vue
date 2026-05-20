<script setup lang="ts">
const props = defineProps<{
  signDate: string | null | undefined
  tooltip?: string
}>()

const signed = computed(() => !!props.signDate)
</script>

<template>
  <v-tooltip :text="signed ? `Firmado: ${signDate}` : (tooltip ?? 'Pendiente de firma')" location="top">
    <template #activator="{ props: tooltipProps }">
      <v-icon
        v-bind="tooltipProps"
        :color="signed ? 'success' : 'grey-lighten-1'"
        size="16"
      >
        {{ signed ? 'mdi-circle' : 'mdi-circle-outline' }}
      </v-icon>
    </template>
  </v-tooltip>
</template>
