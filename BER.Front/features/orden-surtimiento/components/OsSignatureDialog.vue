<script setup lang="ts">
/**
 * Diálogo de firma electrónica (eSign / eFirma).
 *
 * Pendiente de implementación: requiere integración con la librería
 * jsrsasign para validación de certificados X.509 y firma RSA-SHA1,
 * equivalente al sign-widget.js del proyecto legacy.
 *
 * Flujo esperado:
 *   1. Usuario sube archivo .cer (certificado) y .key (llave privada)
 *   2. Se valida que el RFC del certificado coincida con el usuario
 *   3. Se genera la firma digital sobre los documentos seleccionados
 *   4. POST /SupplyOrders/FirmaUnoAsync  → obtener cadena a firmar
 *   5. POST /SupplyOrders/CompletaFirmaAsync → enviar firma
 */

const props = defineProps<{
  modelValue: boolean
  documentos: any[]
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', val: boolean): void
  (e: 'signed'): void
}>()

const dialog = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})
</script>

<template>
  <v-dialog v-model="dialog" max-width="520" persistent>
    <v-card>
      <v-card-title class="text-subtitle-1 font-weight-bold pa-4">
        <v-icon color="warning" class="mr-2">mdi-shield-key</v-icon>
        Firma Electrónica
      </v-card-title>
      <v-divider />
      <v-card-text class="pa-6">
        <v-alert type="warning" variant="tonal" density="compact" class="mb-4">
          <strong>Integración eSign pendiente.</strong><br />
          Este módulo requiere la implementación del componente de firma
          digital con jsrsasign (equivalente al sign-widget.js del legacy).
        </v-alert>
        <p class="text-body-2 text-medium-emphasis">
          Documentos seleccionados: <strong>{{ documentos.length }}</strong>
        </p>
      </v-card-text>
      <v-divider />
      <v-card-actions class="pa-4 justify-end">
        <v-btn variant="text" @click="dialog = false">Cancelar</v-btn>
        <v-btn color="primary" disabled>Firmar</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
