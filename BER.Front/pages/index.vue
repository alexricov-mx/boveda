<script setup lang="ts">
const seguridadStore = useSeguridadStore()
</script>

<template>
  <v-container fluid class="pa-6">
    <v-row>
      <v-col cols="12">
        <h2 class="text-h5 font-weight-bold text-primary mb-1">Bienvenido</h2>
        <p class="text-body-2 text-medium-emphasis">
          Hola, {{ seguridadStore.usuarioAutenticado?.nombre }}. Selecciona un módulo del menú.
        </p>
      </v-col>
    </v-row>
  </v-container>
</template>
